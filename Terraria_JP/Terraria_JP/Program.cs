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
            if (!TitleIsTerraria() && !ExistsBackup())
            {
                MessageBox.Show("����Terraria.exe�����H�ς݂ł��B" + Environment.NewLine + "�I�����܂��B",
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            // �f�t�H���g��Terraria.exe������΁A�V�����A�Z���u�����쐬
            else
            {
                string old = "";
                if (!TitleIsTerraria() && ExistsBackup())
                {
                    old = "�i�o�b�N�A�b�v�g�p�j" + Environment.NewLine;
                    File.Copy("Terraria_old.exe", "Terraria.exe", true);
                }

                var result = MessageBox.Show("Terraria.exe����{�ꉻ���܂��B" + Environment.NewLine + old + "���\�b�O�ォ����܂��B",
                    "����",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);

                // �uOK�v�ȊO�͑S���L�����Z��
                if (result != DialogResult.OK) Environment.Exit(0);

                // �������t�H�[���̕\��
                var thread = new Thread(new ThreadStart(Waiting));
                thread.IsBackground = true;
                thread.Start();

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

        // Terraria_old.exe�����݂��邩���ׂ�
        static bool ExistsBackup()
        {
            return File.Exists("Terraria_old.exe");
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
             * �@(3) ����̊֐��̖����ɑ���p�֐���ǉ�����
             */
            foreach (var type in asm_tera.MainModule.GetTypes())
            {
                if (!type.IsNested) type.IsPublic = true;

                // Program.Main
                if (type.Name == "Program") RenameMethod(type, "Main");
                // Lang.dialog, Lang.npcName, Lang.setLang, Lang.itemName
                else if (type.Name == "Lang")
                {
                    RenameMethod(type, "dialog");
                    RenameMethod(type, "npcName");
                    RenameMethod(type, "setLang");
                    RenameMethod(type, "itemName");
                    RenameMethod(type, "toolTip");
                    RenameMethod(type, "toolTip2");
                    RenameMethod(type, "setBonus");
                }
                // Steam.Init
                else if (type.Name == "Steam") RenameMethod(type, "Init");
                // Item.AffixName
                else if (type.Name == "Item") RenameMethod(type, "AffixName");
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

            if (true)
            {
                Process p = Process.Start(psi);
                p.WaitForExit();
            }

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

            // �ꕔ�̓���ȃN���X������������
            var tail = asm_merge.MainModule.GetType("Terraria.Tail");

            foreach (var type in asm_merge.MainModule.GetTypes())
            {
                if (type.Name == "Main")
                {
                    TailMethod(type, tail, "LoadContent");
                    TailMethod(type, tail, "Initialize");
                }
            }

            var fs3 = new FileStream("Terraria_JP/asm_merge.exe", FileMode.Create);
            asm_merge.Write(fs3);
            fs3.Close();
        }

        static void TailMethod(TypeDefinition type, TypeDefinition tail, string method_name)
        {
            foreach (var method1 in type.Methods)
            {
                if (method1.Name == method_name)
                {
                    foreach (var method2 in tail.Methods)
                    {
                        // �������߂̒ǉ�
                        if (method2.Name == "Tail" + method_name)
                        {
                            var instr1 = method1.Body.Instructions;
                            var last = instr1[instr1.Count - 1];
                            if (last.OpCode == OpCodes.Ret) instr1.Remove(last);

                            foreach (var item in method2.Body.Instructions) instr1.Add(item);
                        }
                        // �������߂ŌĂяo����郁�\�b�h�̒ǉ�
                        else if (method2.Name == "_" + method_name)
                        {
                            var new_method = new MethodDefinition(method2.Name, method2.Attributes, method2.ReturnType);

                            // ���[�J���ϐ��̒ǉ�
                            foreach (var item in method2.Body.Variables) new_method.Body.Variables.Add(item);

                            // ���\�b�h�{�̂̒ǉ�
                            foreach (var item in method2.Body.Instructions) new_method.Body.Instructions.Add(item);

                            // �V���\�b�h��ǉ�
                            type.Methods.Add(new_method);
                        }
                    }
                    return;
                }
            }
        }

        static void RenameMethod(TypeDefinition type, string method_name)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name == method_name)
                {
                    method.IsPublic = true;

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
                    
                    break;
                }
            }
        }

        static void ConcatMethod(MethodDefinition method_to, MethodDefinition method_from)
        {
            var instr1 = method_to.Body.Instructions;

            // �Ōオ���^�[�����߂�������폜����i������������j
            var last = instr1[instr1.Count - 1];
            if (last.OpCode == OpCodes.Ret) instr1.Remove(last);

            var local1 = method_to.Body.Variables;
            foreach (var item in method_from.Body.Variables)
            {
                local1.Add(new VariableDefinition(item.VariableType));
            }

            foreach (var item in method_from.Body.Instructions)
            {
                instr1.Add(item);
            }
        }
    }
#endif
}

