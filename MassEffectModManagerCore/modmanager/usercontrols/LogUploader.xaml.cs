﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ByteSizeLib;
using Flurl.Http;
using MassEffectModManagerCore.modmanager.helpers;
using MassEffectModManagerCore.modmanager.localizations;
using MassEffectModManagerCore.modmanager.me3tweaks;
using MassEffectModManagerCore.modmanager.objects;
using MassEffectModManagerCore.ui;
using Serilog;
using SevenZip;

namespace MassEffectModManagerCore.modmanager.usercontrols
{
    /// <summary>
    /// Interaction logic for LogUploader.xaml
    /// </summary>
    public partial class LogUploader : MMBusyPanelBase
    {
        public bool UploadingLog { get; private set; }
        public string TopText { get; private set; } = M3L.GetString(M3L.string_selectALogToView);
        public ObservableCollectionExtended<LogItem> AvailableLogs { get; } = new ObservableCollectionExtended<LogItem>();
        public LogUploader()
        {
            DataContext = this;
            LoadCommands();
            InitializeComponent();
        }


        private void InitLogUploaderUI()
        {
            AvailableLogs.ClearEx();
            var directory = new DirectoryInfo(App.LogDir);
            var logfiles = directory.GetFiles(@"modmanagerlog*.txt").OrderByDescending(f => f.LastWriteTime).ToList();
            AvailableLogs.AddRange(logfiles.Select(x => new LogItem(x.FullName)));
            if (LogSelector_ComboBox.Items.Count > 0)
            {
                LogSelector_ComboBox.SelectedIndex = 0;
            }
        }

        public ICommand UploadLogCommand { get; set; }
        public ICommand CancelUploadCommand { get; set; }
        public LogItem SelectedLog { get; set; }

        private void LoadCommands()
        {
            UploadLogCommand = new GenericCommand(StartLogUploadManual, CanUploadLog);
            CancelUploadCommand = new GenericCommand(CancelUpload, CanCancelUpload);
        }

        private void StartLogUploadManual()
        {
            StartLogUpload();
        }

        private void CancelUpload()
        {
            OnClosing(DataEventArgs.Empty);
        }

        private bool CanCancelUpload()
        {
            return !UploadingLog;
        }

        private void StartLogUpload(bool isPreviousCrashLog = false)
        {
            UploadingLog = true;
            TopText = M3L.GetString(M3L.string_collectingLogInformation);
            NamedBackgroundWorker bw = new NamedBackgroundWorker(@"LogUpload");
            bw.DoWork += (a, b) =>
            {
                string logUploadText = LogCollector.CollectLogs(SelectedLog.filepath);
                using (var output = new MemoryStream())
                {
                    var encoder = new LzmaEncodeStream(output);
                    using (var normalBytes = new MemoryStream(Encoding.UTF8.GetBytes(logUploadText)))
                    {
                        int bufSize = 24576, count;
                        var buf = new byte[bufSize];

                        while ((count = normalBytes.Read(buf, 0, bufSize)) > 0)
                        {
                            encoder.Write(buf, 0, count);
                        }
                    }

                    encoder.Close();

                    //Upload log to ME3Tweaks

                    var lzmalog = output.ToArray();
                    try
                    {
                        //this doesn't need to technically be async, but library doesn't have non-async method.
                        string responseString = @"https://me3tweaks.com/modmanager/logservice/logupload.php".PostUrlEncodedAsync(new { LogData = Convert.ToBase64String(lzmalog), ModManagerVersion = App.BuildNumber, CrashLog = isPreviousCrashLog }).ReceiveString().Result;
                        Uri uriResult;
                        bool result = Uri.TryCreate(responseString, UriKind.Absolute, out uriResult)
                                      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (result)
                        {
                            //should be valid URL.
                            //diagnosticsWorker.ReportProgress(0, new ThreadCommand(SET_DIAGTASK_ICON_GREEN, Image_Upload));
                            //e.Result = responseString;
                            Log.Information(@"Result from server for log upload: " + responseString);
                            b.Result = responseString;
                            return;
                        }
                        else
                        {
                            Log.Error(@"Error uploading log. The server responded with: " + responseString);
                            b.Result = M3L.GetString(M3L.string_interp_serverRejectedTheUpload, responseString);
                        }
                    }
                    catch (AggregateException e)
                    {
                        Exception ex = e.InnerException;
                        string exmessage = ex.Message;
                        b.Result = M3L.GetString(M3L.string_interp_logWasUnableToUpload, exmessage);
                    }
                    catch (FlurlHttpTimeoutException)
                    {
                        // FlurlHttpTimeoutException derives from FlurlHttpException; catch here only
                        // if you want to handle timeouts as a special case
                        Log.Error(@"Request timed out while uploading log.");
                        b.Result = M3L.GetString(M3L.string_interp_requestTimedOutUploading);

                    }
                    catch (Exception ex)
                    {
                        // ex.Message contains rich details, inclulding the URL, verb, response status,
                        // and request and response bodies (if available)
                        Log.Error(@"Handled error uploading log: " + App.FlattenException(ex));
                        string exmessage = ex.Message;
                        var index = exmessage.IndexOf(@"Request body:");
                        if (index > 0)
                        {
                            exmessage = exmessage.Substring(0, index);
                        }

                        b.Result = M3L.GetString(M3L.string_interp_logWasUnableToUpload, exmessage);
                    }
                }

            };
            bw.RunWorkerCompleted += (a, b) =>
            {
                if (b.Result is string response)
                {
                    if (response.StartsWith(@"http"))
                    {
                        Utilities.OpenWebpage(response);
                    }
                    else
                    {
                        OnClosing(DataEventArgs.Empty);
                        var res = M3L.ShowDialog(Window.GetWindow(this), response, M3L.GetString(M3L.string_logUploadFailed), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                OnClosing(DataEventArgs.Empty);
            };
            bw.RunWorkerAsync();
        }

        private bool CanUploadLog()
        {
            return LogSelector_ComboBox.SelectedItem != null && !UploadingLog;
        }

        public override void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !UploadingLog)
            {
                e.Handled = true;
                OnClosing(DataEventArgs.Empty);
            }
        }

        public override void OnPanelVisible()
        {
            InitLogUploaderUI();
        }

        public class LogItem
        {
            public string filepath;
            public LogItem(string filepath)
            {
                this.filepath = filepath;
            }

            public override string ToString()
            {
                return Path.GetFileName(filepath) + @" - " + ByteSize.FromBytes(new FileInfo(filepath).Length);
            }
        }
    }
}
