using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Шифрование_на_эллиптических_кривых
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    public class Point
    {
        public int x;
        public int y;
        public int order;
        public bool is_O;
        public override string ToString()
        {
            if (is_O)
                return "Point O";
            else
                return "Point (" + x.ToString() + ";" + y.ToString() + "), order =" + order;
        }
        public Point(int x, int y, int order)
        {
            this.x = x;
            this.y = y;
            this.order = order;
            is_O = false;
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            order = -1;
            is_O = false;
        }
        public Point(bool is_O)
        {
            x = 0;
            y = 0;
            order = -1;
            this.is_O = is_O;
        }
        //public static bool operator !=(Point p1, Point p2)
        //{
        //    if (p1.x != p2.x || p1.y != p2.y || p1.is_O != p2.is_O)
        //        return true;
        //    else
        //        return false;
        //}
        //public static bool operator ==(Point p1, Point p2)
        //{
        //    if (p1 != p2)
        //        return false;
        //    else
        //        return true;
        //}
    }
    public class Group
    {
        private int p, a, b;
        private List<Point> group;
        private int index_G = 0; //открытая генерирующая точка
        private int n_A, n_B; //закрытые множители
        private int index_P_A, index_P_B; // открытые сессионные ключи
        private int count_char;

        private int[] sqrs; //sqrs[i] = i^2 mod p
        private int[] sqrts; //sqrts[i] = sqrt(i) mod p
        private Random rand;
        int[] prTo1000 = new int[] {257,263,269,271,277,281,283,293,307,311,313,317,331,337,347,349,353,359,
        367,373,379,383,389,397,401,409,419,421,431,433,439,443,449,457,461,463,467,479,487,491,499,503,509,
        521,523,541,547,557,563,569,571,577,587,593,599,601,607,613,617,619,631,641,643,647,653,659,661,673,
        677,683,691,701,709,719,727,733,739,743,751,757,761,769,773,787,797,809,811,821,823,827,829,839,853,
        857,859,863,877,881,883,887,907,911,919,929,937,941,947,953,967,971,977,983,991,997};

        public Group(int new_p, int new_a, int new_b)
        {
            rand = new Random();
            group = new List<Point>();
            p = new_p;
            a = new_a;
            b = new_b;

            //Вычислим квадраты и квадратные корни чисел по модулю
            sqrs = new int[p]; //sqrs[i] = i^2 mod p
            sqrts = new int[p]; //sqrts[i] = sqrt(i) mod p
            for (int i = 1; i < p; i++)
            {
                sqrs[i] = (i * i).Mod(p);
                if (sqrts[(i * i).Mod(p)] == 0)
                    sqrts[(i * i).Mod(p)] = i;
            }

            //Зададим все точки эллиптической кривой
            for (int x = 0; x < p; x++)
            {
                int y = (x * x * x + x * a + b).Mod(p);
                if (sqrs.Contains(y))
                {
                    group.Add(new Point(x, sqrts[y]));
                    if (sqrts[y] != 0)
                        group.Add(new Point(x, -sqrts[y] + p));
                }
            }
            group.Add(new Point(true));

            //Определим порядок каждой точки
            for (int i = 0; i < group.Count - 1; i++)
            {
                int por = 1;
                Point init = new Point(group[i].x, group[i].y);
                Point result = new Point(group[i].x, group[i].y);
                while (!result.is_O)
                {
                    por++;
                    result = Sum(init, result);
                }
                group[i].order = por;
            }

            //Сколько символов char нужно для записи одной точки в зашифрованном виде?
            count_char = 1;
            int length = group.Count;
            while (length / 256 > 0)
            {
                count_char++;
                length /= 256;
            }
            //int max = -1;
            //for (int i = 0; i < group.Count; i++)
            //{
            //    if (max < group[i].x)
            //        max = group[i].x;
            //    if (max < group[i].y)
            //        max = group[i].y;
            //}
            //while (max > 255)
            //{
            //    max = max / 256;
            //    count_char++;
            //}
            do
            {
                index_G = rand.Next(0, group.Count-1);
            }
            while (!prTo1000.Contains(group[index_G].order));
            /*Delete me!*/
            //index_G = 234;
            n_A = rand.Next(2, group.Count - 1);
            /*Delete me!*/
            //n_A = 5;
            index_P_A = Mult(index_G, n_A);
            n_B = rand.Next(2, group.Count - 1);
            /*Delete me!*/
            //n_B = 11;
            index_P_B = Mult(index_G, n_B);
        }
        public string Encrypt(string text)
        {
            string encrypted = "";

            for (int i = 0; i < text.Length; i++)
            {
                //Перевожу каждый символ в точку Pm
                int index_Pm = CharToIndexInGroup(text[i]);

                //Шифрую точку Pm в точки P1 и P2
                int k = rand.Next(2, group[index_G].order - 1);
                int index_P1 = Mult(index_G, k);
                int index_P2 = Sum(index_Pm, Mult(index_P_B, k));

                //Перевожу точки P1 и P2 в count_char символов char
                encrypted += IndexInGroupToString(index_P1);
                encrypted += IndexInGroupToString(index_P2);
            }
            
            return encrypted;
        }
        private int CharToIndexInGroup(char ch)
        {
            return (int)ch;
        }
        private int CharToIndexInGroup(string str)
        {
            int res = 0;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                res = (int)str[i] + res * 256;
                //СДВИНУТСЯ В ОБРАТНОМ ПОРЯДКЕ
            }
            return res;
        }
        private char IndexInGroupToChar(int index)
        {
            //if (index / 256 > 0)
            //    return (char)0;
            return (char)index;
        }
        private string IndexInGroupToString(int index)
        {
            string res = "";
            for (int i = 0; i < count_char; i++)
            {
                res += (char)(index % 256);
                index /= 256;
                //ПИШУТСЯ В ОБРАТНОМ ПОРЯДКЕ
            }
            return res;
        }
        
        public string Decrypt(string text)
        {
            string decrypted = "";

            for (int i = 0; i < text.Length; i += count_char * 2)
            {
                //Перевожу count_char символов в точку P1
                string substr = "";
                for (int j = 0; j < count_char; j++)
                {
                    substr += text[i + j];
                }
                int index_P1 = CharToIndexInGroup(substr);

                //Перевожу count_char символов в точку P2
                substr = "";
                for (int j = count_char; j < count_char * 2; j++)
                {
                    substr += text[i + j];
                }
                int index_P2 = CharToIndexInGroup(substr);

                //Дешифрую точки P1 и P2 в точку Pm
                int index_buf = Mult(index_P1, n_B);
                Point buf = new Point(group[index_buf].x, group[index_buf].y);
                buf.y = -buf.y; //беру точку со знаком минус
                Point Pm = Sum(group[index_P2], buf);
                int index_Pm = IndexOf(Pm.x, Pm.y);

                //Перевожу точку Pm в символ char
                decrypted += IndexInGroupToChar(index_Pm);
            }

            return decrypted;
        }
        private int Count()
        {
            return group.Count;
        }
        private Point At(int index)
        {
            if (index >= group.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return group[index];
        }
        private void Add(Point point)
        {
            group.Add(point);
        }
        private int IndexOf(int x, int y)
        {
            int i = 0;
            bool end = false;
            for (; i < group.Count && !end; i++)
            {
                if (group[i].x == x && group[i].y == y)
                    end = true;
            }
            if (end)
                return i - 1;
            else
                return -1;
        }
        private int Sum(int index_p1, int index_p2)
        {
            Point p1 = group[index_p1];
            Point p2 = group[index_p2];
            Point sum = Sum(p1, p2);
            return IndexOf(sum.x, sum.y);
        }
        private Point Sum(Point p1, Point p2)
        {
            bool is_O = false;

            int tg = 0;
            if (p1.x == p2.x && p1.y == p2.y)
            {
                int value = (2 * p1.y).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((3 * p1.x * p1.x + a) * value.GetInverse(p)).Mod(p);
            }
            else
            {
                int value = (p2.x - p1.x).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((p2.y - p1.y) * value.GetInverse(p)).Mod(p);
            }

            int x = 0, y = 0;
            if (is_O)
                return new Point(true);
            else
            {
                x = (tg * tg - p1.x - p2.x).Mod(p);
                y = (tg * (p1.x - x) - p1.y).Mod(p);
                return new Point(x, y);
            }
        }
        private int Mult(int indexInGroup, int n)
        {
            Point p1 = new Point(group[indexInGroup].x, group[indexInGroup].y);
            while (n % 2 == 0 && n > 0)
            {
                p1 = Sum(p1, p1);
                n /= 2;
            }

            Point p2 = new Point(p1.x, p1.y);
            for (int i = 1; i < n; i++)
            {
                p1 = Sum(p1, p2);
            }

            return IndexOf(p1.x, p1.y);
        }
    }
    public static class Int32Extensions
    {
        public static int GetInverse(this int a, int p)
        {
            // Реализован расширенный алгоритм Евклида
            int c = a, d = p, u, v;
            int uc = 1, vc = 0, ud = 0, vd = 1;

            while (c != 0)
            {
                int q = d / c;
                int temp;
                temp = c;
                c = d - q * c;
                d = temp;

                temp = uc;
                uc = ud - q * uc;
                ud = temp;

                temp = vc;
                vc = vd - q * vc;
                vd = temp;
            }
            u = ud < 0 ? ud + p : ud;
            v = vd < 0 ? vd + p : vd;

            return (d == 1) ? u : 0;
        }
        public static int PowMod(this int a, int pow, int mod)
        {
            a = a.Mod(mod);
            int res = 1;
            int buf = a;
            for (int i = 1; i <= pow; i *= 2)
            {
                if (i > 1)
                    buf = (buf * buf).Mod(mod);
                if ((pow & i) > 0)
                {
                    res *= buf;
                }
            }
            return res.Mod(mod);
        }
        public static int Mod(this int a, int p)
        {
            return ((a % p) + p) % p;
        }
    }
}