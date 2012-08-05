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
            // TODO: default settings
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: puzzle image - screenshot
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: 3x3 puzzle
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: 4x4 puzzle
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // TODO: puzzle type --> puzzle image
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // TODO: puzzle type <-- puzzle image
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image --> shuffled puzzle solver
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image <-- shuffled puzzle solver
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // TODO: shuffled puzzle solver --> solution
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // TODO: shuffled puzzle solver <-- solution
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // TODO: exit
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image - take a screenshot
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: puzzle image - from disk
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // TODO: puzzle image - browse
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: puzzle image - from web
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
