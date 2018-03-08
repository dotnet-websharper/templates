using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace WebSharper.Vsix
{
    class VSVersionWizard : IWizard
    {
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var dte = (_DTE)automationObject;
            replacementsDictionary.Add("$visualstudioversion$", dte.Version);
            var v = Version.Parse(dte.Version);
            if (v >= new Version(15, 6)) {
                replacementsDictionary.Add("$fsharpcoreversion$", "4.4.3.0");
            }
            else {
                replacementsDictionary.Add("$fsharpcoreversion$", "4.4.1.0");
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
