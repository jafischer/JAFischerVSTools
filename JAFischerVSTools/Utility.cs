using EnvDTE;

namespace JAFischerVSTools
{
    internal class Utility
    {
        internal static void ConvertSelectionToLines(TextSelection sel, out int start_line, out int end_line)
        {
            start_line = sel.TopPoint.Line;
            end_line = sel.BottomPoint.Line;

            sel.MoveToLineAndOffset(start_line, 1);
            sel.MoveToLineAndOffset(end_line, 1, true);
            sel.EndOfLine(true);
        }

        internal static void ReselectLines(TextSelection sel, int start_line, int end_line)
        {
            sel.MoveToLineAndOffset(start_line, 1);
            sel.MoveToLineAndOffset(end_line, 1, true);
            sel.EndOfLine(true);
        }
    }
}
