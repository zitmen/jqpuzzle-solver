using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace jqpuzzle_solver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            // tabs
            tabControl1.Enabled = true;
            tabPage1.Enabled = true;
            tabPage2.Enabled = false;
            tabPage3.Enabled = false;
            tabPage4.Enabled = false;
            // puzzle type
            radioButton2.Enabled = true;
            radioButton1.Enabled = true;
            button1.Enabled = false;
            // puzzle image
            button2.Enabled = false;
            button5.Enabled = false;
            button4.Enabled = false;
            button3.Enabled = false;
            button10.Enabled = false;
            radioButton3.Enabled = false;
            radioButton4.Enabled = false;
            radioButton5.Enabled = false;
            textBox2.Enabled = false;
            textBox1.Enabled = false;
            pictureBox1.Enabled = false;
            label5.Text = "NxN";
            // shuffled puzzle solver
            button6.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            radioButton7.Enabled = false;
            radioButton8.Enabled = false;
            textBox4.Enabled = false;
            pictureBox2.Enabled = false;
            // solution
            label14.Text = "X";
            listBox1.Enabled = false;
            pictureBox3.Enabled = false;
            button7.Enabled = false;
            button11.Enabled = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            EnablePage2(3);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            EnablePage2(4);
        }

        private void EnablePage2(int n)
        {
            button1.Enabled = true;
            tabPage2.Enabled = true;
            radioButton3.Enabled = true;
            radioButton4.Enabled = true;
            radioButton5.Enabled = true;
            pictureBox1.Enabled = true;
            button5.Enabled = true;
            label5.Text = string.Format("{0:d}x{0:d}", n);
            //
            if (radioButton3.Checked)
            {
                textBox1.Enabled = true;
                button4.Enabled = true;
            }
            else if (radioButton4.Checked)
            {
                button3.Enabled = true;
            }
            else if (radioButton5.Checked)
            {
                button10.Enabled = true;
                textBox2.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
            //
            textBox1.Enabled = false;
            button4.Enabled = false;
            button10.Enabled = false;
            textBox2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image - take a screenshot
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            button4.Enabled = true;
            //
            button3.Enabled = false;
            button10.Enabled = false;
            textBox2.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image - browse
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            button10.Enabled = true;
            textBox2.Enabled = true; 
            //
            button3.Enabled = false;
            textBox1.Enabled = false;
            button4.Enabled = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image - download
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: shuffled - from screen
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: shuffled manually
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // TODO: shuffled - screenshot
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: moves - switch to another move -> redraw the board and arrow
        }
    }
}
