using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace Terraria_JP
{
#if WINDOWS || XBOX
    static class Program
    {
        static Waiting form;

        static void Main(string[] args)
        {
            // �f�t�H���g��Terraria.exe�ł͂Ȃ������ꍇ�A�x�����ďI��
            if (!TitleIsTerraria())
            {
                MessageBox.Show("����Terraria.exe�����H�ς݂ł��B" + Environment.NewLine + "�I�����܂��B",
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            // �f�t�H���g��Terraria.exe������΁A�V�����A�Z���u�����쐬
            else
            {
                var result = MessageBox.Show("Terraria.exe����{�ꉻ���܂��B" + Environment.NewLine + "���\�b�O�ォ����܂��B",
                    "����",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);

                // �uOK�v�ȊO�͑S���L�����Z��
                if (result != DialogResult.OK) Environment.Exit(0);

                // �������t�H�[���̕\��
                var thread = new Thread(new ThreadStart(Waiting));
                thread.IsBackground = true;
                thread.Start();

                // �X�v���C�g�t�H���g�̃o�b�N�A�b�v�ƃR�s�[
                var files = Directory.GetFiles("Terraria_JP/Fonts", "*.xnb");
                var font_dir = "Content" + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar;
                Directory.CreateDirectory(font_dir + "old");
                foreach (var file in files)
	            {
                    var file_name = Path.GetFileName(file);
                    File.Copy(font_dir + file_name, font_dir + "old" + Path.DirectorySeparatorChar + file_name, true);
                    File.Copy(file, font_dir + file_name, true);
	            }

                // �A�Z���u���쐬
                MakeAssembly();

                // �������t�H�[�����N���[�Y
                thread.Abort();

                // Terraria.exe���o�b�N�A�b�v���āA�A�Z���u���ŏ㏑��
                File.Copy("Terraria.exe", "Terraria_old.exe", true);
                File.Copy("Terraria_JP/asm_merge.exe", "Terraria.exe", true);
            }

            MessageBox.Show("���{�ꉻ���������܂����B" + Environment.NewLine +
                "�I���W�i���̃t�@�C����Terraria_old.exe�Ƀo�b�N�A�b�v���܂����B",
                "����",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        public static void Waiting()
        {
            form = new Waiting();
            form.Show();

            while (true)
            {
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        // Terraria.exe���I���W�i�������ׂ�
        static bool TitleIsTerraria()
        {
            var terraria = AssemblyDefinition.ReadAssembly("Terraria.exe");
            foreach (var attr in terraria.CustomAttributes)
            {
                // �^�C�g�������擾
                if (attr.AttributeType.Name == "AssemblyTitleAttribute")
                {
                    var title = (string)attr.ConstructorArguments[0].Value;
                    return (title == "Terraria");
                }
            }
            return false;
        }

        static void MakeAssembly()
        {
            // �e�A�Z���u���̓ǂݍ���
            var asm_base = AssemblyDefinition.ReadAssembly("Terraria_JP/BaseAssembly.exe");
            var asm_tera = AssemblyDefinition.ReadAssembly("Terraria.exe");

            /*
             * Terraria�̃A�Z���u���Ɉȉ��̉��H���s��
             * �@(1) �S�ẴN���X��public�ɂ���
             * �@(2) ����̊֐������l�[������public�ɂ���
             */
            foreach (var type in asm_tera.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;

                // Program.Main()�����l�[��
                if (type.Name == "Program")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "Main")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
                // Lang.dialog()�����l�[��
                else if (type.Name == "Lang")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "dialog")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
                // Steam.Init()�����l�[��
                else if (type.Name == "Steam")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "Init")
                        {
                            method.IsPublic = true;
                            MethodDup(type, method);
                            break;
                        }
                    }
                }
            }

            // �x�[�X�A�Z���u���̑S�ẴN���X��public�ɂ���
            foreach (var type in asm_base.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;
            }

            // ���H�����A�Z���u�����ꎞ�I�ɏo�͂���
            var fs1 = new FileStream("Terraria_JP/asm_base.exe", FileMode.Create);
            asm_base.Write(fs1);
            fs1.Close();
            asm_base = null;
            var fs2 = new FileStream("Terraria_JP/asm_tera.exe", FileMode.Create);
            asm_tera.Write(fs2);
            fs2.Close();
            asm_tera = null;

            // �ꎞ�A�Z���u�����}�[�W����
            //ProcessStartInfo psi = new ProcessStartInfo("Terraria_JP/ILRepack.exe", "/union /ndebug /out:Terraria_JP/asm_merge.exe Terraria_JP/asm_base.exe Terraria_JP/asm_tera.exe");
            ProcessStartInfo psi = new ProcessStartInfo("Terraria_JP/ILRepack.exe", "/union /ndebug /parallel /out:Terraria_JP/asm_merge.exe Terraria_JP/asm_base.exe Terraria_JP/asm_tera.exe");

            psi.RedirectStandardOutput = false;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process p = Process.Start(psi);
            //var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            //Console.WriteLine("Output: " + Environment.NewLine + output);

            // �ꎞ�A�Z���u�����폜����
            if (true)
            {
                File.Delete("Terraria_JP/asm_base.exe");
                File.Delete("Terraria_JP/asm_tera.exe");
            }

            // �o�͂����A�Z���u���̃A�[�L�e�N�`����ύX����
            var asm_merge = AssemblyDefinition.ReadAssembly("Terraria_JP/asm_merge.exe");
            foreach (var mod in asm_merge.Modules)
            {
                mod.Architecture = TargetArchitecture.I386;
                mod.Attributes |= ModuleAttributes.Required32Bit;
            }
            var fs3 = new FileStream("Terraria_JP/asm_merge.exe", FileMode.Create);
            asm_merge.Write(fs3);
            fs3.Close();
        }

        static void MethodDup(TypeDefinition type, MethodDefinition method)
        {
            var new_method = new MethodDefinition("_" + method.Name, method.Attributes, method.ReturnType);

            // �p�����[�^�̃R�s�[
            foreach (var par in method.Parameters)
            {
                new_method.Parameters.Add(new ParameterDefinition(par.ParameterType));
            }

            // ���[�J���ϐ��̃R�s�[
            foreach (var variable in method.Body.Variables)
            {
                new_method.Body.Variables.Add(new VariableDefinition(variable.VariableType));
            }

            // ���\�b�h�{�̂̃R�s�[
            var il = new_method.Body.GetILProcessor();
            foreach (var instr in method.Body.Instructions)
            {
                il.Append(instr);
            }

            // �V�������\�b�h��ǉ�����
            type.Methods.Add(new_method);
            return;
        }
    }
#endif
}

