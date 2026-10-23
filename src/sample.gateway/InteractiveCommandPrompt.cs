namespace sample.gateway;

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;

internal static class InteractiveCommandPrompt
{
    public static int Invoke(RootCommand rootCommand, string[] args, TextReader input, TextWriter output)
    {
        if (args.Length == 0)
        {
            args = ReadArguments(rootCommand, input, output);
            if (args == null)
            {
                output.WriteLine("Command cancelled.");
                return 0;
            }
        }

        return rootCommand.Parse(args).Invoke();
    }

    private static string[] ReadArguments(RootCommand rootCommand, TextReader input, TextWriter output)
    {
        var commands = rootCommand.Subcommands.Where(command => !command.Hidden).ToArray();
        output.WriteLine("Choose a command (enter its number or name; q to cancel):");
        for (int i = 0; i < commands.Length; i++)
        {
            output.WriteLine($"{i + 1}. {commands[i].Name} {commands[i].Description}");
        }

        Command selectedCommand;
        while (true)
        {
            output.Write("Command: ");
            string selection = input.ReadLine()?.Trim();
            if (selection == null || selection.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            selectedCommand = commands.FirstOrDefault(command => command.Name.Equals(selection, StringComparison.OrdinalIgnoreCase));
            if (selectedCommand == null && int.TryParse(selection, out int index) && index > 0 && index <= commands.Length)
            {
                selectedCommand = commands[index - 1];
            }
            if (selectedCommand != null)
            {
                break;
            }
            output.WriteLine("Select a command from the list.");
        }

        var arguments = new List<string> { selectedCommand.Name };
        output.WriteLine("Enter q at any prompt to cancel. Press Enter to keep an optional value's default.");
        foreach (var option in selectedCommand.Options.Where(option => !option.Hidden))
        {
            output.WriteLine($"{option.Name}: {option.Description}");
            var valueType = Nullable.GetUnderlyingType(option.ValueType) ?? option.ValueType;
            if (valueType.IsEnum)
            {
                output.WriteLine($"Choices: {string.Join(", ", Enum.GetNames(valueType))}");
            }

            object defaultValue = option.HasDefaultValue ? option.GetDefaultValue() : null;
            string hint = option.Required ? "required" : defaultValue != null
                ? $"default: {Convert.ToString(defaultValue, CultureInfo.InvariantCulture)}"
                : valueType == typeof(bool) ? "true/false; default: false" : "optional";

            while (true)
            {
                output.Write($"{option.Name} [{hint}]: ");
                string value = input.ReadLine()?.Trim();
                if (value == null || value.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                if (value.Length == 0)
                {
                    if (option.Required)
                    {
                        output.WriteLine("A value is required.");
                        continue;
                    }
                    break;
                }

                string argument = $"{option.Name}={value}";
                var parsed = rootCommand.Parse([.. arguments, argument]);
                if (parsed.UnmatchedTokens.Count > 0)
                {
                    output.WriteLine($"Invalid value for {option.Name}: {value}");
                    continue;
                }
                var optionResult = parsed.GetResult(option);
                var errors = parsed.Errors.Where(error => IsForOption(error.SymbolResult, optionResult)).ToArray();
                if (errors.Length > 0)
                {
                    foreach (var error in errors)
                    {
                        output.WriteLine(error.Message);
                    }
                    continue;
                }

                arguments.Add(argument);
                break;
            }
        }

        return arguments.ToArray();
    }

    private static bool IsForOption(SymbolResult result, OptionResult optionResult)
    {
        for (; result != null; result = result.Parent)
        {
            if (result == optionResult)
            {
                return true;
            }
        }
        return false;
    }
}
