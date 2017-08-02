//------------------------------------------------------------------------------
// <copyright file="AlignAssignments.cs" company="SCEA">
//     Copyright (c) SCEA.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace JAFischerVSTools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AlignAssignments
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e01e327a-6e14-4295-a2bb-b51b17235763");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private readonly DTE2 dte;
        private const string blanks = "                                                                                                                                     ";
        private readonly Regex assignmentRegex = new Regex("=[^=]");

        /// <summary>
        /// Initializes a new instance of the <see cref="AlignAssignments"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AlignAssignments(Package package)
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
        public static AlignAssignments Instance
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
            Instance = new AlignAssignments(package);
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
            TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;

            int start_line, end_line;
            Utility.ConvertSelectionToLines(sel, out start_line, out end_line);

            // Tabs break the column calculations.
            dte.ExecuteCommand("Edit.UntabifySelectedLines");

            sel.ReplacePattern("^([^=]*[^ ]) *=", "$1=", (int) vsFindOptions.vsFindOptionsRegularExpression);

            // Figure out the alignment column.
            int alignment_column = 0;
            for (var cur_line = start_line; cur_line <= end_line; ++cur_line)
            {
                sel.GotoLine(cur_line, true);

                string line = sel.Text;
                alignment_column = Math.Max(alignment_column, assignmentRegex.Match(line).Index);
            }

            // Now go through and adjust the comments.
            for (var cur_line = start_line; cur_line <= end_line; ++cur_line)
            {
                sel.GotoLine(cur_line, true);

                string line = sel.Text;
                int pos = assignmentRegex.Match(line).Index;
                if (pos != -1)
                {
                    line = line.Substring(0, pos) + blanks.Substring(0, alignment_column - pos + 1) + line.Substring(pos);
                    sel.Insert(line);
                }
            }

            Utility.ReselectLines(sel, start_line, end_line);
        }
    }
}
