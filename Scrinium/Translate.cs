using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;

namespace Scrinium
{
    public class Translate
    {
        public string GetTranslation(string word)
        {
            string dir = System.IO.Path.GetDirectoryName(@"RunTranslation");
            ScriptEngine engine = Python.CreateEngine();
            ICollection<string> paths = engine.GetSearchPaths();

            paths.Add("OpenWords/open_words");

            engine.SetSearchPaths(paths);

            ScriptScope scope = engine.CreateScope();

            //scope.SetVariable("word", word);
            //engine.ExecuteFile(@"RunTranslation.py");
            return "";
            //return scope.GetVariable<string>("translation");
        }
    }
}
