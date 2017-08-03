void foo()
{
	if (commandService != null)
	{
		var menuCommandID = new CommandID(CommandSet, CommandId);
		var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);


		commandService.AddCommand(menuItem);
	}
}

	a = 1; // 12
	asdf = 2; // 15
    ab = 3; // 13
	no_comment = 3.5;
    a = b = (c == d); // 23
	something = rotten; // 25

#define some_macro(asdf) \
  line1; \
  another_line; \
  last_line;
