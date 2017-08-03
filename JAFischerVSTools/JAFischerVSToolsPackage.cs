//------------------------------------------------------------------------------
// <copyright file="JAFischerVSToolsPackage.cs" company="SCEA">
//     Copyright (c) SCEA.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace JAFischerVSTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(JAFischerVSToolsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // jafischer: Added this to get the package to load on startup:
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public sealed class JAFischerVSToolsPackage : Package
    {
        /// <summary>
        /// JAFischerVSToolsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c9d11715-eb34-4edc-826d-a51d2cf59f28";

        private readonly Regex alphanumeric = new Regex(@"[a-zA-Z0-9]");
        private DTE2 dte;
        private TextDocumentKeyPressEvents keyPressEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="JAFischerVSToolsPackage"/> class.
        /// </summary>
        public JAFischerVSToolsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            setupSmartHyphen();

            TodoComment.Initialize(this);
            AlignAssignments.Initialize(this);
            AlignComments.Initialize(this);
            AlignTrailingBackSlashes.Initialize(this);
            ToggleComment.Initialize(this);
        }

        private void setupSmartHyphen()
        {
            this.dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

            // Because Visual Studio no longer allows you to bind a command to a normal key press like "-", we have to
            // do it via the TextDocumentKeyPressEvents.BeforeKeyPress event. Boo.
            this.keyPressEvents = (dte.Events as Events2).TextDocumentKeyPressEvents;
            this.keyPressEvents.BeforeKeyPress += onBeforeKeyPress;
        }

        private void onBeforeKeyPress(string keyPress, TextSelection selection, bool inStatementCompletion, ref bool cancelKeyPress)
        {
            if (keyPress == "-" && selection.CurrentColumn > 1)
            {
                if (selection.ActivePoint.AtStartOfLine)
                    return;

                selection.CharLeft(true);
                string previousChar = selection.Text;

                cancelKeyPress = alphanumeric.IsMatch(previousChar);

                // If "-" is pressed twice in a row, then we want to change the "_" back to a "-", so keep the previous character selected.
                if (previousChar != "_")
                    selection.CharRight();

                // If preceding character is part of a symbol, then convert the "-" to "_".
                if (cancelKeyPress)
                    selection.Insert("_");
            }
            else if ((keyPress == ">" || keyPress == ".") && selection.CurrentColumn > 1)
            {
                // If "->" is typed, we want to undo the conversion of the "-" to "_". And while we're at it,
                // do the same for "-." since we're all about avoiding the evil shift key.
                selection.CharLeft(true);
                string previousChar = selection.Text;
                cancelKeyPress = previousChar == "_";
                if (cancelKeyPress)
                    selection.Insert("->");
                else
                    selection.CharRight();
            }
        }
        #endregion
    }
}
