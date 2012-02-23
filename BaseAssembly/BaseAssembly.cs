using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;

namespace Terraria
{
    public static class Program
    {
        public static Settings Setting;

        private static void Main(string[] args)
        {
            Setting = Settings.ReadSetting("Terraria_JP/Settings.xml");

            // XML�����\�[�X����Ăяo��
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream("Terraria.language.xml");

            try
            {
                var local = new FileStream("Terraria_JP/language.xml", FileMode.Open);
                if (local != null) stream = local;
            }
            catch
            {
            }

            // XML�t�@�C����ǂݍ���
            Ja.xml = new XmlDocument();
            Ja.xml.Load(stream);

            // ���X��Main�֐����Ăяo��
            var type = typeof(Terraria.Program);
            var method = type.GetMethod("_Main");
            method.Invoke(type, new Object[]{args});

            return;
        }
    }

    public static class Ja
    {
        const int MAX_NUM = 27;
        public static XmlDocument xml;
        

        public static string GetDialog(int l)
        {
            if (xml == null) return "";

            var xpath = "/language/lang/dialogs";
            var parent = xml.DocumentElement.SelectSingleNode(xpath);
            foreach (XmlElement node in parent.ChildNodes)
            {
                if (node["int"] != null)
                {
                    if (int.Parse(node["int"].InnerText) == l)
                    {
                        if (node["ja"] != null)
                        {
                            var temp = node["ja"].InnerText;

                            // �S�p�󔒕����͉��s�R�[�h�ɕϊ�����
                            temp = temp.Replace('�@', '\n');

                            // �e�L�����N�^�[�̖��O��ϊ�����
                            var type_main = Type.GetType("Terraria.Main");
                            var fld_player = type_main.GetField("player");
                            var fld_myPlayer = type_main.GetField("myPlayer");
                            var fld_chrName = type_main.GetField("chrName");
                            var type_player = Type.GetType("Terraria.Player");
                            var fld_name = type_player.GetField("name");

                            var chrName = (string[])fld_chrName.GetValue(null);
                            var myPlayer = (int)fld_myPlayer.GetValue(null);
                            var player = (Object[])fld_player.GetValue(null);
                            var name = (string)fld_name.GetValue(player[myPlayer]);

                            Console.WriteLine(temp);
                            temp = temp.Replace("{Player}", name);
                            temp = temp.Replace("{Nurse}", chrName[18]);
                            temp = temp.Replace("{Mechanic}", chrName[124]);
                            temp = temp.Replace("{Demolitionist}", chrName[38]);
                            temp = temp.Replace("{Guide}", chrName[22]);
                            temp = temp.Replace("{Merchant}", chrName[17]);
                            temp = temp.Replace("{Arms Dealer}", chrName[19]);
                            temp = temp.Replace("{Dryad}", chrName[20]);
                            temp = temp.Replace("{Goblin}", chrName[107]);

                            // ���s�R�[�h�ň�s���Ƃɕ�����
                            var strs = temp.Split(new char[] { '\n' });

                            // �P���C�����Ƃɏ������Ă����A�P�s�Q�T�����O��𒴂����玩���ŉ��s��}��
                            for (int x = 0; x < strs.Length; x++)
                            {
                                if (strs[x].Length <= MAX_NUM) continue;

                                int i = 0;
                                int j = 0;
                                bool flag_hankaku = false;
                                while (true)
                                {
                                    if (i >= strs[x].Length) break;

                                    // ���p�����̊Ԃ͉��s���Ȃ�
                                    flag_hankaku = (0x20 <= strs[x][i] && strs[x][i] <= 0x7D);

                                    // ���p�����łȂ��A�ő啶�����𒴂��Ă�������s
                                    if (!flag_hankaku && (j/2) >= (MAX_NUM - 1))
                                    {
                                        strs[x] = strs[x].Insert(i, "\n");
                                        Console.WriteLine("�P���C���������F" + strs[x]);
                                        j = 0;
                                        i += 2;
                                        continue;
                                    }

                                    i++;

                                    // ���p�Ȃ�P�������A�S�p�Ȃ�Q�������Ƃ��ăJ�E���g
                                    if (flag_hankaku) j++;
                                    else j += 2;
                                }
                            }

                            // �Ō�ɉ��s�R�[�h�ŘA��
                            var new_text = string.Join("\n", strs);

                            Console.WriteLine(new_text);
                            return new_text;
                        }
                        else return "";
                    }
                }
            }

            return "";
        }
    }

    public class Lang
    {
        public static string dialog(int l)
        {
            // �I���W�i���̃e�L�X�g���擾
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_dialog");
            var str_origin = (string)method.Invoke(null, new object[]{l});

            // XML��̃e�L�X�g���擾
            var str_ja = Ja.GetDialog(l);

            // ��łȂ����̃e�L�X�g��Ԃ�
            return (str_ja == "") ? str_origin : str_ja;
        }
    }

    public class Steam
    {
        public static bool SteamInit;

        public static void Init()
        {
            if (Program.Setting.NoSteam)
            {
                Steam.SteamInit = true;
            }
            else
            {
                var type = typeof(Terraria.Steam);
                var method = type.GetMethod("_Init");
                method.Invoke(null, null);
            }
        }
    }
}