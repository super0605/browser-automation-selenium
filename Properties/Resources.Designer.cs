// Decompiled with JetBrains decompiler
// Type: CaseDownloader.Properties.Resources
// Assembly: CaseDownloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3F07DDC4-44AA-4D83-9E77-3391A6F7DE19
// Assembly location: F:\Client-task\2017.1207\workspace20171212\CaseDownloader (2)\CaseDownloader\CaseDownloader.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CaseDownloader.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (CaseDownloader.Properties.Resources.resourceMan == null)
          CaseDownloader.Properties.Resources.resourceMan = new ResourceManager("CaseDownloader.Properties.Resources", typeof (CaseDownloader.Properties.Resources).Assembly);
        return CaseDownloader.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return CaseDownloader.Properties.Resources.resourceCulture;
      }
      set
      {
        CaseDownloader.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
