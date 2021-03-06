﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using poshsecframework.Strings;

namespace poshsecframework.PShell
{
    class pscript
    {
        #region " Private Variables "
        private RunspaceConfiguration rspaceconfig;
        private Runspace rspace;
        private String scriptcommand;
        private bool iscommand = false;
        private bool clicked = true;
        private bool scheduled = false;
        private List<psparameter> scriptparams = new List<psparameter>();
        private StringBuilder rslts = new StringBuilder();
        private psexception psexec = new psexception();
        private bool cancel = false;
        private frmMain frm = null;
        private ListViewItem scriptlvw;
        private psvariables.PSRoot PSRoot;
        private psvariables.PSModRoot PSModRoot;
        private psvariables.PSFramework PSFramework;
        private psvariables.PSExec PSExec;
        private psmethods.PSMessageBox PSMessageBox;
        private psmethods.PSAlert PSAlert;
        private psmethods.PSStatus PSStatus;
        private psmethods.PSHosts PSHosts;
        private psmethods.PSTab PSTab;
        private String loaderrors = "";
        #endregion

        #region " Public Events "
        public EventHandler<pseventargs> ScriptCompleted;
        #endregion

        #region " Private Methods "
        private void InitializeScript()
        {
            rspaceconfig = RunspaceConfiguration.Create();
            rspace = RunspaceFactory.CreateRunspace(rspaceconfig);
            rspace.Open();
            InitializeSessionVars();
        }

        private void InitializeSessionVars()
        {
            if (rspace.RunspaceAvailability == RunspaceAvailability.Available)
            {
                PSAlert = new psmethods.PSAlert(frm);
                PSRoot = new psvariables.PSRoot("PSRoot");
                PSStatus = new psmethods.PSStatus(frm, scriptlvw);
                PSModRoot = new psvariables.PSModRoot("PSModRoot");
                PSFramework = new psvariables.PSFramework("PSFramework");
                PSExec = new psvariables.PSExec("PSExec");
                PSMessageBox = new psmethods.PSMessageBox();
                PSTab = new psmethods.PSTab(frm);
                PSHosts = new psmethods.PSHosts(frm);
                rspace.SessionStateProxy.PSVariable.Set(PSRoot);
                rspace.SessionStateProxy.PSVariable.Set(PSModRoot);
                rspace.SessionStateProxy.PSVariable.Set(PSFramework);
                rspace.SessionStateProxy.PSVariable.Set(PSExec);
                rspace.SessionStateProxy.SetVariable("PSMessageBox", PSMessageBox);
                rspace.SessionStateProxy.SetVariable("PSAlert", PSAlert);
                rspace.SessionStateProxy.SetVariable("PSStatus", PSStatus);
                rspace.SessionStateProxy.SetVariable("PSHosts", PSHosts);
                rspace.SessionStateProxy.SetVariable("PSTab", PSTab);
            }
        }

        private void ImportPSFramework()
        {
            if (System.IO.File.Exists(poshsecframework.Properties.Settings.Default.FrameworkPath))
            {
                Pipeline pline = rspace.CreatePipeline();
                pline.Commands.AddScript(StringValue.ImportPSFramework + StringValue.WriteError);
                Collection<PSObject> rslt = pline.Invoke();
                pline.Dispose();
                pline = null;
                if (rslt != null && rslt.Count > 0)
                {
                    foreach (PSObject po in rslt)
                    {
                        if (po != null)
                        {
                            rslts.AppendLine(po.ToString());
                        }
                    }
                    loaderrors += rslts.ToString();
                }
            }
            else
            {
               loaderrors += StringValue.FrameworkFileError;
            }            
        }

        private void InvokeCommand(string command)
        {
            Collection<PSObject> rslt = null;
            Pipeline pline = rspace.CreatePipeline();
            pline.Commands.AddScript(command);           
            try
            {
                rslt = pline.Invoke();
            }
            catch (Exception e)
            {
                rslts.AppendLine(e.Message);
            }
            finally
            {
                pline.Dispose();
                pline = null;
                if (rslt != null)
                {
                    foreach (PSObject po in rslt)
                    {
                        if (po != null)
                        {
                            rslts.AppendLine(po.ToString());
                        }
                    }
                }
                GC.Collect();
            }            
        }
        #endregion

        #region " Public Methods "
        public pscript()
        {            
            try
            {
                InitializeScript();
                ImportPSFramework();
            }
            catch (Exception e)
            {
                //Base Exception Handler
                OnScriptComplete(new pseventargs(StringValue.UnhandledException + Environment.NewLine + e.Message + Environment.NewLine + "Stack Trace:" + Environment.NewLine + e.StackTrace, null, false));
            } 
        }

        public void Dispose()
        {
            rspace.Close();
            rspace.Dispose();
            rspace = null;
        }

        public Collection<PSObject> GetCommand()
        {
            Collection<PSObject> rslt = null;
            String scrpt = "";
            Pipeline pline = rspace.CreatePipeline();
            scrpt = StringValue.GetCommand;
            pline.Commands.AddScript(scrpt);
            try
            {
                rslt = pline.Invoke();
            }
            catch (Exception e)
            {
                rslts.AppendLine(e.Message);
            }
            finally
            {
                pline.Dispose();
                pline = null;
                pline = null;
                if (rslt != null)
                {
                    foreach (PSObject po in rslt)
                    {
                        if (po != null)
                        {
                            rslts.AppendLine(po.ToString());
                        }
                    }
                }
                GC.Collect();
            }                        
            return rslt;
        }

        public bool UnblockFiles(string FolderPath)
        {
            bool rtn = false;
            rslts.Clear();

            if (Directory.Exists(FolderPath))
            {
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
                }
                catch (Exception e)
                {
                    rslts.AppendLine(e.Message);
                }
                if (files != null && files.Count() > 0)
                {
                    string script = "";
                    foreach (string file in files)
                    {
                        script += "Unblock-File -path \"" + file + "\"\r\n";
                    }
                    InvokeCommand(script);
                    if (rslts.ToString().Trim() == "")
                    {
                        rtn = true;
                    }
                }
                else
                {
                    rslts.AppendLine("Unable to find any PowerShell files in the directory " + FolderPath + " or it's subdirectories.");
                }
            }
            else
            {
                rslts.AppendLine("The path " + FolderPath + " does not exist.");
            }
            return rtn;
        }

        public bool SetExecutionPolicy()
        {
            bool rtn = false;
            InvokeCommand(StringValue.SetExecutionPolicy);
            if (rslts.ToString().Trim() == "")
            {
                rtn = true;
            }
            return rtn;
        }

        public bool UpdateHelp()
        {
            bool rtn = false;
            InvokeCommand(StringValue.UpdateHelp);
            if (rslts.ToString().Trim() == "")
            {
                rtn = true;
            }
            return rtn;
        }

        public void RunScript()
        {
            InitializeSessionVars();
            PSAlert.ScriptName = scriptcommand.Replace(poshsecframework.Properties.Settings.Default.ScriptPath, "");            
            Pipeline pline = null;
            bool cancelled = false;
            try
            {
                if (clicked && !scheduled)
                {
                    //Only run this if the user double clicked a script or command.
                    //If they typed the command then they should have passed params.
                    //If it was scheduled and there were params, they should be passed.
                    scriptparams = CheckForParams(scriptcommand);
                }                
                if (!cancel)
                {
                    Command pscmd = new Command(scriptcommand);
                    String cmdparams = "";
                    if (scriptparams != null)
                    {                        
                        foreach (psparameter param in scriptparams)
                        {
                            CommandParameter prm = new CommandParameter(param.Name, param.Value ?? param.DefaultValue);
                            pscmd.Parameters.Add(prm);
                            cmdparams += " -" + param.Name + " " + param.Value;
                        }
                    }

                    pline = rspace.CreatePipeline();
                    if (iscommand)
                    {
                        String cmdscript = scriptcommand + cmdparams;
                        if (clicked)
                        {
                            rslts.AppendLine(scriptcommand + cmdparams);
                        }
                        pline.Commands.AddScript(cmdscript);
                        pline.Commands.Add(StringValue.OutString);
                    }
                    else
                    {
                        rslts.AppendLine("Running script: " + scriptcommand.Replace(poshsecframework.Properties.Settings.Default.ScriptPath, ""));                        
                        pline.Commands.Add(pscmd);
                    }                    
                    Collection<PSObject> rslt = pline.Invoke();
                    pline.Dispose();
                    pline = null;
                    if (rslt != null)
                    {
                        foreach (PSObject po in rslt)
                        {
                            if (po != null)
                            {
                                rslts.AppendLine(po.ToString());
                            }
                        }
                    }
                }
                else
                {
                    rslts.AppendLine(StringValue.ScriptCancelled);
                }
            }
            catch (ThreadAbortException thde)
            {
                if (pline != null)
                {
                    pline.Stop();
                    pline.Dispose();
                    pline = null;
                }
                GC.Collect();
                cancelled = true;
                if (iscommand)
                {
                    rslts.AppendLine(StringValue.CommandCancelled + Environment.NewLine + thde.Message);
                }
                else
                {
                    rslts.AppendLine(StringValue.ScriptCancelled + Environment.NewLine + thde.Message);
                }                
            }
            catch (Exception e)
            {
                rslts.AppendLine(psexec.psexceptionhandler(e,iscommand));
            }
            finally
            {
                if (pline != null)
                {
                    pline.Dispose();
                }
                pline = null;
                GC.Collect();
                OnScriptComplete(new pseventargs(rslts.ToString(), scriptlvw, cancelled));
                rslts.Clear();
            }            
        }

        public List<psparameter> CheckForParams(String scriptcommand)
        {
            cancel = false;
            List<psparameter> parms = null;
            psparamtype parm = new psparamtype();

            Pipeline pline = rspace.CreatePipeline();

            String scrpt = "";
            if (iscommand)
            {
                scrpt = StringValue.GetHelpFull.Replace("{0}", scriptcommand);
            }
            else
            {
                scrpt = StringValue.GetHelpFull.Replace("{0}", "\"" + scriptcommand + "\"");
            }
            pline.Commands.AddScript(scrpt);
            pline.Commands.Add(StringValue.OutString);

            Collection<PSObject> rslt = pline.Invoke();
            if (rslt != null)
            {
                if (rslt[0].ToString().Contains("PARAMETERS"))
                {
                    String[] lines = rslt[0].ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    if (lines != null)
                    {
                        int idx = 0;
                        bool found = false;
                        List<String> fileparams = GetEditorParams(rslt[0].ToString(), "psfilename");
                        List<String> hostparams = GetEditorParams(rslt[0].ToString(), "pshosts");                        
                        do
                        {
                            String line = lines[idx];
                            if (line == "PARAMETERS")
                            {
                                found = true;
                            }
                            idx++;
                        } while (found == false && idx < lines.Length);

                        if (found)
                        {
                            String line = "";
                            do
                            {
                                line = lines[idx];
                                if (line.Trim() != "" && line.Trim().Substring(0, 1) == "-")
                                {
                                    psparameter prm = new psparameter();
                                    String param = line.Trim().Substring(1, line.Trim().Length - 1);
                                    String[] paramparts = param.Split(' ');
                                    prm.Name = paramparts[0].Trim();
                                    if (paramparts.Length == 2)
                                    {
                                        prm.Type = GetTypeFromString(paramparts[1]);
                                    }
                                    else
                                    {
                                        prm.Type = typeof(int);
                                    }
                                    idx++;
                                    line = lines[idx];
                                    prm.Description = line.Trim();
                                    idx += 2;
                                    line = lines[idx];
                                    if (line.Contains("true"))
                                    {
                                        prm.Category = "Required";
                                    }
                                    else
                                    {
                                        prm.Category = "Optional";
                                    }
                                    idx += 2;
                                    line = lines[idx];
                                    if (line.Contains("Default value"))
                                    {
                                        String defval = line.Replace("Default value", "").Trim();
                                        if (defval != "")
                                        {
                                            if (defval.ToLower() == "true" || defval.ToLower() == "false")
                                            {
                                                prm.DefaultValue = bool.Parse(defval);
                                            }
                                            else
                                            {
                                                prm.DefaultValue = defval;
                                            }
                                        }
                                    }
                                    if (fileparams.Contains(prm.Name))
                                    {
                                        prm.IsFileName = true;
                                    }
                                    if (hostparams.Contains(prm.Name))
                                    {
                                        prm.IsHostList = true;
                                    }
                                    parm.Properties.Add(prm);
                                }
                                idx++;
                            } while (line.Substring(0, 1) == " " && idx < lines.Length);
                        }
                    }
                }
            }
            pline.Stop();
            pline.Dispose();
            pline = null;
            GC.Collect();
            if (parm.Properties.Count > 0)
            {
                if (frm.ShowParams(parm) == System.Windows.Forms.DialogResult.OK)
                {
                    parms = parm.Properties;
                }
                else
                {
                    cancel = true;
                }
            }
            return parms;
        }
        #endregion

        #region " Private Methods "
        private Type GetTypeFromString(String typename)
        {
            Type rtn = null;
            switch (typename.ToLower())
            { 
                case "<string>":
                    rtn = typeof(string);
                    break;
                case "<boolean>":
                    rtn = typeof(Boolean);
                    break;
                case "<int32>":
                    rtn = typeof(int);
                    break;
                case "<psobject>":
                    rtn = typeof(PSObject);
                    break;
                case "[<switchparameter>]":
                    rtn = typeof(bool);
                    break;
                default:
                    rtn = typeof(Object);
                    break;
            }
            return rtn;
        }

        private List<String> GetEditorParams(String helptext, String identifier)
        {
            List<String> rtn = new List<string>();
            identifier += "=";
            if (helptext.Contains(identifier))
            {
                int fnidx = helptext.IndexOf(identifier);
                int fnendidx = helptext.IndexOf(" ", fnidx);
                String fnparms = helptext.Substring(fnidx, fnendidx - fnidx);
                fnparms = fnparms.Replace("\r\n", "").Replace(identifier, "");
                if (fnparms.Trim() != "")
                {
                    String[] prms = fnparms.Split(',');
                    if (prms != null && prms.Length > 0)
                    {
                        foreach (String prm in prms)
                        {
                            rtn.Add(prm);
                        }
                    }
                }
            }
            return rtn;
        }

        private void OnScriptComplete(pseventargs e)
        {
            EventHandler<pseventargs> handler = ScriptCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region " Public Properties "
        public String Script
        {
            set { this.scriptcommand = value;  }
        }

        public bool IsCommand
        {
            set { this.iscommand = value; }
        }

        public bool IsScheduled
        {
            set { this.scheduled = value; }
        }

        public bool Clicked
        {
            set { this.clicked = value; }
        }

        public List<psparameter> Parameters
        {
            set { this.scriptparams = value; }
            get { return this.scriptparams; }
        }

        public String Results
        {
            get { return this.rslts.ToString(); }
        }

        public frmMain ParentForm
        {
            set { frm = value; }
        }

        public ListViewItem ScriptListView
        {
            set { scriptlvw = value; }
        }

        public bool ParamSelectionCancelled
        {
            get { return cancel; }
        }

        public String LoadErrors
        {
            get { return loaderrors; }
        }
        #endregion
    }
}
