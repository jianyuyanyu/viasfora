﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using Winterdom.Viasfora.Languages;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio;

namespace Winterdom.Viasfora {
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
  [InstalledProductRegistration("#110", "#111", "1.0", IconResourceID = 400)]
  [Guid(Guids.VSPackage)]
  [ProvideOptionPage(typeof(Options.GeneralOptionsPage), "Viasfora", "General", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.PresentationModeOptionsPage), "Viasfora", "Presentation Mode", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.AllLanguagesOptionsPage), "Viasfora", "Languages", 0, 0, false)]
  [ProvideOptionPage(typeof(Options.CSharpOptionsPage), "Viasfora\\Languages", "C#", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.CppOptionsPage), "Viasfora\\Languages", "C/C++", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.JScriptOptionsPage), "Viasfora\\Languages", "JavaScript", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.VBOptionsPage), "Viasfora\\Languages", "Basic", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.FSharpOptionsPage), "Viasfora\\Languages", "F#", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.SqlOptionsPage), "Viasfora\\Languages", "SQL", 0, 0, true)]
  [ProvideOptionPage(typeof(Options.TypeScriptOptionsPage), "Viasfora\\Languages", "TypeScript", 0, 0, true)]
  [ProvideMenuResource(1000, 1)]
  public sealed class VsfPackage : Package {

    private static List<LanguageInfo> languageList;
    public static VsfPackage Instance { get; private set; }

    public static bool PresentationModeTurnedOn { get; set; }
    public static EventHandler PresentationModeChanged { get; set; }

    static VsfPackage() {
      languageList = new List<LanguageInfo>();
      languageList.Add(new Cpp());
      languageList.Add(new CSharp());
      languageList.Add(new JScript());
      languageList.Add(new VB());
      languageList.Add(new FSharp());
      languageList.Add(new Sql());
      languageList.Add(new TypeScript());
    }

    public static LanguageInfo LookupLanguage(IContentType contentType) {
      foreach ( LanguageInfo li in languageList ) {
        if ( li.MatchesContentType(contentType) )
          return li;
      }
      return null;
    }

    public static int GetPresentationModeZoomLevel() {
      return PresentationModeTurnedOn
        ? VsfSettings.PresentationModeEnabledZoomLevel
        : VsfSettings.PresentationModeDefaultZoomLevel;
    }

    protected override void Initialize() {
      base.Initialize();
      Instance = this;
      Trace.WriteLine("Initializing VsfPackage");

      OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if ( null != mcs ) {
        InitializeViewMenuCommands(mcs);
      }
    }
    private void InitializeViewMenuCommands(OleMenuCommandService mcs) {
      var viewPresentationModeCmdId = new CommandID(
        new Guid(Guids.guidVsfViewCmdSet), 
        PkgCmdIdList.cmdidPresentationMode);
      var viewPresentationModeItem = new OleMenuCommand(OnViewPresentationMode, viewPresentationModeCmdId);
      viewPresentationModeItem.BeforeQueryStatus += OnViewPresentationModeBeforeQueryStatus;
      mcs.AddCommand(viewPresentationModeItem);
    }

    private void OnViewPresentationMode(object sender, EventArgs e) {
      PresentationModeTurnedOn = !PresentationModeTurnedOn;
      if ( PresentationModeChanged != null ) {
        PresentationModeChanged(this, EventArgs.Empty);
      }
    }
    private void OnViewPresentationModeBeforeQueryStatus(object sender, EventArgs e) {
      var cmd = (OleMenuCommand)sender;
      SetViewPresentationModeCmdStatus(cmd);
    }

    private void SetViewPresentationModeCmdStatus(OleMenuCommand cmd) {
      cmd.Checked = PresentationModeTurnedOn;
      cmd.Enabled = VsfSettings.PresentationModeEnabled;
    }
  }
}
