using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace LuaDependencyFinder
{
    internal class CommandManager
    {
        private readonly Dictionary<string, Command> m_commandLookup;
        private IEnumerable<(Command, IEnumerable<string>)>? m_commandGrouping;
        private bool m_isLatest;

        public int LongestCommand { get; private set; }

        public IEnumerable<(Command, IEnumerable<string>)> Commands
        {
            get
            {
                if (m_commandGrouping == null || !m_isLatest)
                {
                    m_commandGrouping = m_commandLookup
                    .GroupBy(x => x.Value.CommandText)
                    .Select(x =>
                    {
                        var command = x.First().Value;
                        var keys = x.Select(x => x.Key);
                        return (command, keys);
                    })
                    .OrderBy(x => x.command.CommandText)
                    .ToList();
                }

                return m_commandGrouping;
            }
        }

        public CommandManager()
        {
            m_commandLookup = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddCommand(Command command)
        {
            m_isLatest = false;
            var commandText = command.CommandText;
            var firstLetter = commandText.Substring(0, 1);

            m_commandLookup.Add(commandText, command);
            if (!m_commandLookup.ContainsKey(firstLetter))
            {
                m_commandLookup.Add(firstLetter, command);
            }

            LongestCommand = Math.Max(LongestCommand, commandText.Length);
        }

        public bool TryGetAction(string key, [NotNullWhen(true)] out Command? command)
        {
            return m_commandLookup.TryGetValue(key, out command);
        }
    }

    internal class Command
    {
        private object m_target;

        public string CommandText { get; }

        public string Details { get; }

        public bool IsAsync { get; }

        public Command(string command, Action target, string details)
        {
            CommandText = command;
            m_target = target;
            Details = details;
            IsAsync = false;
        }

        public Command(string command, Func<Task> target, string details)
        {
            CommandText = command;
            m_target = target;
            Details = details;
            IsAsync = true;
        }

        public async Task Invoke()
        {
            if (IsAsync)
            {
                await ((Func<Task>)m_target).Invoke();
            }
            else
            {
                ((Action)m_target).Invoke();
            }
        }
    }

}
