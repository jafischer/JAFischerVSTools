//------------------------------------------------------------------------------
// <copyright file="ToggleComment.cs" company="SCEA">
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
    internal sealed class ToggleComment
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4132;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e01e327a-6e14-4295-a2bb-b51b17235763");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private readonly DTE2 dte;
        private readonly Regex firstNonSpaceRegex = new Regex(@"[^ ]");

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleComment"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ToggleComment(Package package)
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
        public static ToggleComment Instance
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
            Instance = new ToggleComment(package);
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

            // TODO: jafischer-2017-08-02 For now, just C++ // comments, but eventually could create a map of file extension to comment prefix.

            // Figure out (a) if all lines are commented, and (b) what column the comment prefix should go in if not.
            int comment_column = 999999;
            bool all_lines_commented = true;
            for (int cur_line = start_line; cur_line <= end_line; ++cur_line)
            {
                sel.GotoLine(cur_line, true);
                string line = sel.Text;

                var match = firstNonSpaceRegex.Match(line);
                if (match.Success)
                {
                    int pos = match.Index;
                    comment_column = Math.Min(comment_column, pos);
                    all_lines_commented = all_lines_commented && line.Substring(pos).StartsWith("//");
                }
            }

            for (int cur_line = start_line; cur_line <= end_line; ++cur_line)
            {
                sel.GotoLine(cur_line, true);
                string line = sel.Text;

                // Are we adding comments, or removing?
                if (all_lines_commented)
                {
                    var match = firstNonSpaceRegex.Match(line);
                    if (match.Success)
                    {
                        line = line.Substring(0, comment_column) + line.Substring(comment_column + 2);
                        sel.Insert(line);
                    }
                }
                else
                {
                    // If line is empty, add leading spaces.
                    if (line.Length < comment_column)
                    {
                        // Just overwrite the line, since we know that it is empty or is all blanks. (It must be, due to the regex above).
                        line = Utility.Blanks.Substring(0, comment_column);
                    }

                    line = line.Substring(0, comment_column) + "//" + line.Substring(comment_column);
                    sel.Insert(line);
                }
            }

            Utility.ReselectLines(sel, start_line, end_line);
            if (all_lines_commented)
                dte.ExecuteCommand("Edit.FormatSelection");
        }
    }
}
