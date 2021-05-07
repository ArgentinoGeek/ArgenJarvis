using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface ITwitchCommand
    {
        Task<string> Execute(string commandName, string userName, IList<string> arguments, string argumentsAsString);
    }
}
