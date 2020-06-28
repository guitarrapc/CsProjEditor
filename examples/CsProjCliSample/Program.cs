using System;

namespace CsProjCliSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var edit = new csprojcli.EditCsProj();
            edit.LoadAndRun("uwp_storepublish.json");
        }
    }
}
