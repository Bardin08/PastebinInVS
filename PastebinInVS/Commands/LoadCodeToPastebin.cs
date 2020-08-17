﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

using Task = System.Threading.Tasks.Task;

using PastebinInVS.Structs;
using Microsoft.VisualStudio.Shell.Interop;

namespace PastebinInVS.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LoadCodeToPastebin
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f9aa38ea-6f22-4bff-bf61-9e552eb7a537");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadCodeToPastebin"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LoadCodeToPastebin(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LoadCodeToPastebin Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService))
                .ConfigureAwait(false) as OleMenuCommandService;
            Instance = new LoadCodeToPastebin(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DeveloperApiKey) || string.IsNullOrWhiteSpace(Settings.Default.DeveloperApiKey))
            {
                VsShellUtilities.ShowMessageBox(
                package,
                "Incorrect developer api key. You can change it in a settings.",
                "Error",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return;
            }

            var name = GetActiveFileNameAsync(ServiceProvider).Result;
            var code = GetSelectionAsync(ServiceProvider).Result.Text;

            _ = PastebinPasteRequestAsync(name, code);
        }

        /// <summary>
        /// Get open file name
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Active file name</returns>
        private async Task<string> GetActiveFileNameAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            EnvDTE80.DTE2 applicationObject = await serviceProvider.GetServiceAsync(typeof(DTE)).ConfigureAwait(false) as EnvDTE80.DTE2;
            return applicationObject.ActiveDocument.Name;
        }

        /// <summary>
        /// Paste selected code to pastebin.com
        /// </summary>
        /// <param name="name">Paste name</param>
        /// <param name="code">Paste code</param>
        private async Task PastebinPasteRequestAsync(string name, string code)
        {
            var baseAddress = new Uri("https://pastebin.com/api/api_post.php");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_option", "paste"),
                    new KeyValuePair<string, string>("api_user_key", Settings.Default.UserApiKey),
                    new KeyValuePair<string, string>("api_paste_private", Settings.Default.PastePrivate.ToString()),
                    new KeyValuePair<string, string>("api_paste_name", name),
                    new KeyValuePair<string, string>("api_paste_expire_date", Settings.Default.PasteExpireTime),
                    new KeyValuePair<string, string>("api_paste_format", Settings.Default.PasteLanguage),
                    new KeyValuePair<string, string>("api_dev_key", Settings.Default.DeveloperApiKey),
                    new KeyValuePair<string, string>("api_paste_code", code)
                });

                cookieContainer.Add(baseAddress, new Cookie("CookieName", "cookie_value"));
                var result = await client.PostAsync(baseAddress, content);
                Console.WriteLine(result.IsSuccessStatusCode);
                Console.WriteLine(result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
                if (result.IsSuccessStatusCode)
                {
                    new TextCopy.Clipboard().SetText(result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
                }
            }
        }

        /// <summary>
        /// Get selected text
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Selected text</returns>
        private async Task<TextViewSelection> GetSelectionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            var service = await serviceProvider.GetServiceAsync(typeof(SVsTextManager)).ConfigureAwait(false);
            var textManager = service as IVsTextManager2;
            IVsTextView view;
            _ = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);

            view.GetSelectedText(out string selectedText);

            return new TextViewSelection(start, end, selectedText);
        }
    }
}
