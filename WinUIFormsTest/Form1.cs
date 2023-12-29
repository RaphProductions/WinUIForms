using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Windows.Forms;

namespace WinUIFormsTest;

public partial class Form1 : Form
{
    public Form1() : base()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, System.EventArgs e)
    {
    }

    private void button1_Click(object sender, System.EventArgs e)
    {

    }

    private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        Environment.Exit(0);
    }
}
