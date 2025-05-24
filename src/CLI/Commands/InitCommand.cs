using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Commands
{
    public class InitCommand : IGitCommand
    {
        public string Name => "init";

        public async Task ExecuteAsync(string[] args)
        {
            // Your .git directory setup logic here
            Console.WriteLine("Initialized empty Git repository.");
            await Task.CompletedTask;
        }
    }
}
