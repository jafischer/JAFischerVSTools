//------------------------------------------------------------------------------
// <copyright file="TodoComment.cs" company="SCEA">
//     Copyright (c) SCEA.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;

namespace JAFischerVSTools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TodoComment
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e01e327a-6e14-4295-a2bb-b51b17235763");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private readonly DTE2 dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoComment"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private TodoComment(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            this.dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TodoComment Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new TodoComment(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string userName = System.Environment.GetEnvironmentVariable("USERNAME");
            // I guess Visual Studio runs on Mac & Linux now, doesn't it? So let's check for "USER" as well.
            if (userName == null)
                userName = System.Environment.GetEnvironmentVariable("USER");

            TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
            DateTime now = DateTime.Now;
            sel.Insert(string.Format("// TODO: {0}-{1}-{2:00}-{3:00} ", userName, now.Year, now.Month, now.Day));
        }
    }
}
