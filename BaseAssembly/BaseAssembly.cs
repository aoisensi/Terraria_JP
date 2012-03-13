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
            var xml = new XmlDocument();
            xml.Load(stream);

            // XML�̓��e�𕶎���ɕۑ�����
            var xpath = "/language/lang";
            var node_lang = xml.DocumentElement.SelectSingleNode(xpath);
            Ja.language = new Dictionary<string,Dictionary<int,string>>();

            // �e�탉���Q�[�W�f�[�^�iitems, prefixs�Ȃǁj��ǂݍ���ł���
            foreach (XmlNode node1 in node_lang.ChildNodes)
            {
                var index = node1.LocalName;
                var dic = new Dictionary<int, string>();
                Ja.language.Add(index, dic);

                // �ʂ̃����Q�[�W�f�[�^�iitem, prefix�Ȃǁj��ǂݍ���
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    // �ʂ̍��ڂ��J���Ŗ�����΁A�����ɒǉ�
                    if (node2["int"] != null)
                    {
                        int i = 0;
                        if (int.TryParse(node2["int"].InnerText, out i))
                        {
                            if (node2["ja"] != null)
                            {
                                dic.Add(i, node2["ja"].InnerText);
                            }
                        }
                    }
                }
            }

            // XML�����
            xml = null;

            // ���X��Main�֐����Ăяo��
            var type = typeof(Terraria.Program);
            var method = type.GetMethod("_Main");
            method.Invoke(type, new object[] { args });

            return;
        }
    }

    public static class Ja
    {
        const int MAX_NUM = 27;
        public static Dictionary<string, Dictionary<int, string>> language;

        public static string GetDialog(int l)
        {
            if (language == null) return "";

            // �������擾
            Dictionary<int, string> dic;
            if (language.TryGetValue("dialogs", out dic))
            {
                // �e�L�X�g���擾
                var temp = "";
                if (dic.TryGetValue(l, out temp))
                {
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
                        bool not_newline = false;
                        while (true)
                        {
                            if (i >= strs[x].Length) break;
                            char c = strs[x][i];

                            // ���p�����̊Ԃ͉��s���Ȃ�
                            not_newline = (0x20 <= c && c <= 0x7D);

                            // �֑������̊Ԃ͉��s���Ȃ�
                            not_newline = not_newline || c == '�B' || c == '�A' || c == '�u' || c == '�v';

                            // ���p�����łȂ��A�ő啶�����𒴂��Ă�������s
                            if (!not_newline && (j / 2) >= (MAX_NUM - 1))
                            {
                                strs[x] = strs[x].Insert(i, "\n");
                                Console.WriteLine("�P���C���������F" + strs[x]);
                                j = 0;
                                i += 2;
                                continue;
                            }

                            i++;

                            // ���p�Ȃ�P�������A�S�p�Ȃ�Q�������Ƃ��ăJ�E���g
                            if (not_newline) j++;
                            else j += 2;
                        }
                    }

                    // �Ō�ɉ��s�R�[�h�ŘA��
                    var new_text = string.Join("\n", strs);
                    return new_text;
                }
            }
            return "";
        }

        public static string GetNpcName(int l)
        {
            if (language == null) return "";

            // �������擾
            Dictionary<int, string> dic;
            if (language.TryGetValue("npcnames", out dic))
            {
                // �e�L�X�g���擾
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        public static string GetItemName(int l)
        {
            if (language == null) return "";

            // �������擾
            Dictionary<int, string> dic;
            if (language.TryGetValue("items", out dic))
            {
                // �e�L�X�g���擾
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        public static string GetPrefix(int l)
        {
            if (language == null) return "";

            // �������擾
            Dictionary<int, string> dic;
            if (language.TryGetValue("prefixs", out dic))
            {
                // �e�L�X�g���擾
                var name = "";
                if (dic.TryGetValue(l, out name))
                {
                    return name;
                }
            }
            return "";
        }

        // Terraria.Lang��static�����o�[�ɒl���Z�b�g����
        public static void setLang(Type type)
        {
            if (language == null) return;

            var strs = new string[] { "misc", "menu", "gen", "inter", "tip"};
            foreach (var str in strs)
            {
                // ���݂��鎫���̂ݎ擾
                Dictionary<int, string> dic;
                if (language.TryGetValue(str+"s", out dic))
                {
                    foreach (var pair in dic)
                    {
                        if (pair.Key < 0)
                        {
                            Console.WriteLine("�L�[�l���}�C�i�X�ł��B�L�[�F{0}�@�o�����[�F{1}", pair.Key, pair.Value);
                            continue;
                        }
                        type.InvokeMember(str, BindingFlags.SetField, null, null, new object[] { pair.Key, pair.Value });
                    }
                }
                else
                {
                    Console.WriteLine("�����f�[�^��������܂���F" + str);
                }
            }
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

        public static string npcName(int l)
        {
            // �I���W�i���̃e�L�X�g���擾
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_npcName");
            var str_origin = (string)method.Invoke(null, new object[] { l });

            // XML��̃e�L�X�g���擾
            var str_ja = Ja.GetNpcName(l);

            // ��łȂ����̃e�L�X�g��Ԃ�
            return (str_ja == "") ? str_origin : str_ja;
        }

        public static void setLang()
        {
            // �I���W�i���̃e�L�X�g���擾
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_setLang");
            var str_origin = (string)method.Invoke(null, null);

            // XML��̃e�L�X�g��ݒ�
            Ja.setLang(type);
        }

        public static string itemName(int l)
        {
            // �I���W�i���̃e�L�X�g���擾
            var type = typeof(Terraria.Lang);
            var method = type.GetMethod("_itemName");
            var str_origin = (string)method.Invoke(null, new object[] { l });

            // XML��̃e�L�X�g���擾
            var str_ja = Ja.GetItemName(l);

            // ��łȂ����̃e�L�X�g��Ԃ�
            return (str_ja == "") ? str_origin : str_ja;
        }
    }

    public class Item
    {
        public string AffixName()
        {
            // �I���W�i���̃e�L�X�g���擾
            var type = typeof(Terraria.Item);
            var method = type.GetMethod("_AffixName");
            var str_origin = (string)method.Invoke(this, null);

            // �v���t�B�N�X�ƃA�C�e���̃t�B�[���h���擾
            var f_prefix = type.GetField("prefix");
            var f_name = type.GetField("name");

            // �t�B�[���h����l���擾
            var prefix = Ja.GetPrefix((byte)f_prefix.GetValue(this));
            var name = (string)f_name.GetValue(this);

            // ���O����łȂ���΁A���{���Ԃ�
            if (name != "")
            {
                if (prefix != "") return name + "�i" + prefix + "�j";
                else return name;
            }

            // ���O����Ȃ�΁A���̖��O��Ԃ�
            return str_origin;
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
                Console.WriteLine("Steam�������̊֐�");
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
