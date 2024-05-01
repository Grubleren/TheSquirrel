using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TheSquirrel
{
    // Token represents peaces of of text in a line separated by white space
    // A token text is split into head, body, tail
    // Body may be a number as string (int or float), the number value is stored in number
    // numberValid s true if body is a number (makes it possible to sort numbers)
    public class Token : IComparable
    {
        TheSquirrel theSquirrel;
        List<string> options;
        public string text;
        int lineNumber;
        StreamWriter logWriter;
        public string head;
        public string body;
        public string tail;
        public double number;
        public bool numberValid;
        public int tokenNumber;

        // Split text into head, body, tail an check if body is a number 
        public Token(TheSquirrel theSquirrel, List<string> options, string text, int lineNumber, StreamWriter logWriter)
        {
            this.theSquirrel = theSquirrel;
            this.options = options;
            this.text = text;
            this.lineNumber = lineNumber;
            this.logWriter = logWriter;

            Split();
        }

        // Split the token
        void Split()
        {
            char nfds = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];

            int i = 0;
            while (i < text.Length)
            {
                if (char.IsDigit(text[i]))
                    break;
                i++;
            }
            int bodyStart = i;
            head = text.Substring(0, bodyStart);

            i = text.Length - 1;
            while (i >= 0)
            {
                if (char.IsDigit(text[i]))
                    break;
                i--;
            }
            int bodyEnd = i;
            tail = text.Substring(bodyEnd + 1);
            if (bodyStart <= bodyEnd)
                body = text.Substring(bodyStart, bodyEnd - bodyStart + 1);
            else
                body = "";
            if (head.Length > 0)
            {
                if (head[head.Length - 1] == nfds)
                {
                    head = head.Substring(0, head.Length - 1);
                    body = nfds + body;
                }
            }
            if (tail.Length > 0)
            {
                if (tail[0] == nfds)
                {
                    body += tail[0];
                    tail = tail.Substring(1);
                }
            }

            bool ok;
            int nm;
            ok = int.TryParse(body, out nm);
            number = nm;
            if (!ok)
            {
                ok = double.TryParse(body, out number);
                if (!ok)
                {
                    numberValid = false;
                }
                else
                    numberValid = true;
            }
            else
                numberValid = true;

        }

        // Compare two tokens, number compare is used for the body if possible
        public int CompareTo(object obj)
        {
            Token Token1 = obj as Token;

            int compare;

            if (numberValid && Token1.numberValid)
            {
                compare = compareText(head, Token1.head);
                if (compare == 0)
                    compare = number > Token1.number ? 1 : number == Token1.number ? 0 : -1;
                if (compare == 0)
                    compare = compareText(tail, Token1.tail);
            }
            else
                compare = compareText(text, Token1.text);


            return compare;
        }

        // Compare two strings of text by converting each char to an integer 
        public int compareText(string text1, string text2)
        {
            int length = Math.Min(text1.Length, text2.Length);
            int compare = 0;
            for (int i = 0; i < length; i++)
            {
                compare = Convert(text1[i]) > Convert(text2[i]) ? 1 : Convert(text1[i]) == Convert(text2[i]) ? 0 : -1;
                if (compare != 0)
                    break;
            }
            if (compare == 0)
                compare = text1.Length > text2.Length ? 1 : text1.Length == text2.Length ? 0 : -1;
            return compare;
        }

        // Convert char to integer. Could be a table in a .ini file
        int Convert(int x)
        {
            if (x < 0 || x > 255)
                x = 0;
            int y = convert[x, 1];

            if (!theSquirrel.vOption(options))
                if (y >= 105 && y <= 133)
                    y += 29;

            return y;
        }

        static byte[,] convert = new byte[,]{

  {      0,    0 },
  {      1,    0 },
  {      2,    0 },
  {      3,    0 },
  {      4,    0 },
  {      5,    0 },
  {      6,    0 },
  {      7,    0 },
  {      8,    0 },
  {      9,    0 },
  {     10,    0 },
  {     11,    0 },
  {     12,    0 },
  {     13,    0 },
  {     14,    0 },
  {     15,    0 },
  {     16,    0 },
  {     17,    0 },
  {     18,    0 },
  {     19,    0 },
  {     20,    0 },
  {     21,    0 },
  {     22,    0 },
  {     23,    0 },
  {     24,    0 },
  {     25,    0 },
  {     26,    0 },
  {     27,    0 },
  {     28,    0 },
  {     29,    0 },
  {     30,    0 },
  {     31,    0 },
  {     32,    0 },
  {     33,   63 }, //         !
  {     34,   64 }, //         "
  {     35,   65 }, //         #
  {     36,   66 }, //         $
  {     37,   67 }, //         %
  {     38,   68 }, //         &
  {     39,   69 }, //         '
  {     40,   70 }, //         (
  {     41,   71 }, //         )
  {     42,   72 }, //         *
  {     43,   73 }, //         +
  {     44,   74 }, //         ,
  {     45,   75 }, //         -
  {     46,   76 }, //         .
  {     47,   77 }, //         /
  {     48,   95 }, //         0
  {     49,   96 }, //         1
  {     50,   97 }, //         2
  {     51,   98 }, //         3
  {     52,   99 }, //         4
  {     53,  100 }, //         5
  {     54,  101 }, //         6
  {     55,  102 }, //         7
  {     56,  103 }, //         8
  {     57,  104 }, //         9
  {     58,   78 }, //         :
  {     59,   79 }, //         ;
  {     60,   80 }, //         <
  {     61,   81 }, //         =
  {     62,   82 }, //         >
  {     63,   83 }, //         ?
  {     64,   84 }, //         @
  {     65,  105 }, //         A
  {     66,  106 }, //         B
  {     67,  107 }, //         C
  {     68,  108 }, //         D
  {     69,  109 }, //         E
  {     70,  110 }, //         F
  {     71,  111 }, //         G
  {     72,  112 }, //         H
  {     73,  113 }, //         I
  {     74,  114 }, //         J
  {     75,  115 }, //         K
  {     76,  116 }, //         L
  {     77,  117 }, //         M
  {     78,  118 }, //         N
  {     79,  119 }, //         O
  {     80,  120 }, //         P
  {     81,  121 }, //         Q
  {     82,  122 }, //         R
  {     83,  123 }, //         S
  {     84,  124 }, //         T
  {     85,  125 }, //         U
  {     86,  126 }, //         V
  {     87,  127 }, //         W
  {     88,  128 }, //         X
  {     89,  129 }, //         Y
  {     90,  130 }, //         Z
  {     91,   85 }, //         [
  {     92,   86 }, //         \
  {     93,   87 }, //         ]
  {     94,   88 }, //         ^
  {     95,   89 }, //         _
  {     96,   90 }, //         `
  {     97,  134 }, //         a
  {     98,  135 }, //         b
  {     99,  136 }, //         c
  {    100,  137 }, //         d
  {    101,  138 }, //         e
  {    102,  139 }, //         f
  {    103,  140 }, //         g
  {    104,  141 }, //         h
  {    105,  142 }, //         i
  {    106,  143 }, //         j
  {    107,  144 }, //         k
  {    108,  145 }, //         l
  {    109,  146 }, //         m
  {    110,  147 }, //         n
  {    111,  148 }, //         o
  {    112,  149 }, //         p
  {    113,  150 }, //         q
  {    114,  151 }, //         r
  {    115,  152 }, //         s
  {    116,  153 }, //         t
  {    117,  154 }, //         u
  {    118,  155 }, //         v
  {    119,  156 }, //         w
  {    120,  157 }, //         x
  {    121,  158 }, //         y
  {    122,  159 }, //         z
  {    123,   91 }, //         {
  {    124,   92 }, //         |
  {    125,   93 }, //         }
  {    126,   94 }, //         ~
  {    127,    0 },
  {    128,    0 }, //         €
  {    129,    0 },
  {    130,    1 }, //         ‚
  {    131,    2 }, //         ƒ
  {    132,    3 }, //         „
  {    133,    4 }, //         …
  {    135,    6 }, //         ‡
  {    136,    7 }, //         ˆ
  {    137,    8 }, //         ‰
  {    134,    5 }, //         †
  {    138,    9 }, //         Š
  {    139,   10 }, //         ‹
  {    140,   11 }, //         Œ
  {    141,    0 },
  {    142,   12 }, //         Ž
  {    143,    0 },
  {    144,    0 },
  {    146,   14 }, //         ’
  {    145,   13 }, //         ‘
  {    147,   15 }, //         “
  {    148,   16 }, //         ”
  {    149,   17 }, //         •
  {    150,   18 }, //         –
  {    151,   19 }, //         —
  {    152,   20 }, //         ˜
  {    153,   21 }, //         ™
  {    154,   22 }, //         š
  {    155,   23 }, //         ›
  {    156,   24 }, //         œ
  {    157,    0 },
  {    158,   25 }, //         ž
  {    159,   26 }, //         Ÿ
  {    160,    0 },
  {    161,   27 }, //         ¡
  {    163,   29 }, //         £
  {    162,   28 }, //         ¢
  {    164,   30 }, //         ¤
  {    165,   31 }, //         ¥
  {    166,   32 }, //         ¦
  {    167,   33 }, //         §
  {    168,   34 }, //         ¨
  {    169,   35 }, //         ©
  {    170,   36 }, //         ª
  {    171,   37 }, //         «
  {    172,   38 }, //         ¬
  {    173,    0 },
  {    174,   39 }, //         ®
  {    175,   40 }, //         ¯
  {    176,   41 }, //         °
  {    177,   42 }, //         ±
  {    178,   43 }, //         ²
  {    179,   44 }, //         ³
  {    180,   45 }, //         ´
  {    181,   46 }, //         µ
  {    182,   47 }, //         ¶
  {    183,   48 }, //         ·
  {    184,   49 }, //         ¸
  {    185,   50 }, //         ¹
  {    186,   51 }, //         º
  {    187,   52 }, //         »
  {    188,   53 }, //         ¼
  {    189,   54 }, //         ½
  {    190,   55 }, //         ¾
  {    191,   56 }, //         ¿
  {    192,  105 }, //         À
  {    193,  105 }, //         Á
  {    194,  105 }, //         Â
  {    195,  105 }, //         Ã
  {    196,  131 }, //         Ä
  {    197,  133 }, //         Å
  {    198,  131 }, //         Æ
  {    199,  107 }, //         Ç
  {    200,  109 }, //         È
  {    201,  109 }, //         É
  {    202,  109 }, //         Ê
  {    203,  109 }, //         Ë
  {    204,  113 }, //         Ì
  {    205,  113 }, //         Í
  {    206,  113 }, //         Î
  {    207,  113 }, //         Ï
  {    208,  108 }, //         Ð
  {    209,  118 }, //         Ñ
  {    210,  119 }, //         Ò
  {    211,  119 }, //         Ó
  {    212,  119 }, //         Ô
  {    213,  119 }, //         Õ
  {    214,  132 }, //         Ö
  {    215,   57 },	//         ×
  {    216,  132 }, //         Ø
  {    217,  125 }, //         Ù
  {    218,  125 }, //         Ú
  {    219,  125 }, //         Û
  {    220,  129 }, //         Ü
  {    221,  129 }, //         Ý
  {    222,   58 }, //         Þ
  {    223,   59 }, //         ß
  {    224,  134 }, //         à
  {    225,  134 }, //         á
  {    226,  134 }, //         â
  {    227,  134 }, //         ã
  {    228,  160 }, //         ä
  {    229,  162 }, //         å
  {    230,  160 }, //         æ
  {    231,  136 }, //         ç
  {    232,  138 }, //         è
  {    233,  138 }, //         é
  {    234,  138 }, //         ê
  {    235,  138 }, //         ë
  {    236,  142 }, //         ì
  {    237,  142 }, //         í
  {    238,  142 }, //         î
  {    239,  142 }, //         ï
  {    240,  137 }, //         ð
  {    241,  147 }, //         ñ
  {    242,  148 }, //         ò
  {    243,  148 }, //         ó
  {    244,  148 }, //         ô
  {    245,  148 }, //         õ
  {    246,  161 }, //         ö
  {    247,   60 }, //         ÷
  {    248,  161 }, //         ø
  {    249,  154 }, //         ù
  {    250,  154 }, //         ú
  {    251,  154 }, //         û
  {    252,  158 }, //         ü
  {    253,  158 }, //         ý
  {    254,   61 }, //         þ
  {    255,   62 }};//         ÿ
    }
}
