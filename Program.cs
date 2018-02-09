// Decompiled with JetBrains decompiler
// Type: CaseDownloader.Program
// Assembly: CaseDownloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3F07DDC4-44AA-4D83-9E77-3391A6F7DE19
// Assembly location: F:\Client-task\2017.1207\workspace20171212\CaseDownloader (2)\CaseDownloader\CaseDownloader.exe

using System;
using System.Windows.Forms;

namespace CaseDownloader
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new frmCaseDownloader());
    }
  }
}
