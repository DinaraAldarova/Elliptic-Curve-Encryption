using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Шифрование_на_эллиптических_кривых
{
    public partial class Form1 : Form
    {
        //Эллиптическая кривая
        const int p = 449;
        const int a = 1, b = 3; //опорная точка
        Group group;
        string encrypted_text;

        public Form1()
        {
            InitializeComponent();
            group = new Group(p, a, b);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = richTextBox1.Text;
            
            encrypted_text = group.Encrypt(text);
            richTextBox2.Text = encrypted_text;
            richTextBox3.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = encrypted_text;
            
            richTextBox3.Text = group.Decrypt(text);
        }
    }
}
