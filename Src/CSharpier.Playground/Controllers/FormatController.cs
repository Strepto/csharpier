using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CSharpier;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CSharpier.Playground.Controllers
{
    public class FormatResult
    {
        public string Code { get; set; }
        public string Json { get; set; }
        public string Doc { get; set; }
        public List<FormatError> Errors { get; set; }
    }

    public class FormatError
    {
        public FileLinePositionSpan LineSpan { get; set; }
        public string Description { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class FormatController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger logger;
        private readonly PlaygroundOptions options;

        // ReSharper disable once SuggestBaseTypeForParameter
        public FormatController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<FormatController> logger,
            IOptions<PlaygroundOptions> options)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            this.options = options.Value;
        }

        [HttpPost]
        public FormatResult Post([FromBody]string content)
        {
            var filePath = Path.Combine(
                this.webHostEnvironment.ContentRootPath,
                "App_Data",
                "Uploads",
                content.CalculateHash() + ".cs");
            new FileInfo(filePath).EnsureDirectoryExists();
            // TODO 2 we need to report back errors and what not
            // failing to compile/parse with roslyn
            // what about when the prettier plugin fails because of missing node types or other errors?
            this.WriteAllText(filePath, content);
            // TODO 2 we also want to eventually expose options
            var result = new CodeFormatter().Format(
                content,
                new CSharpier.Options
                {
                    IncludeAST = true,
                    IncludeDocTree = true,

                });

            var formattedFilePath = filePath.Replace(".cs", ".Formatted.cs");
            this.WriteAllText(formattedFilePath, result.Code);

            return new FormatResult
            {
                Code = result.Code,
                Json = result.AST,
                Doc = result.DocTree,
                Errors = result.Errors.Select(ConvertError).ToList(),

            };
        }

        private FormatError ConvertError(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.SourceTree.GetLineSpan(diagnostic.Location.SourceSpan);
            return new FormatError
            {
                LineSpan = lineSpan,
                Description = diagnostic.ToString(),
            };
        }

        private string ReadAllText(string filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }

        private void WriteAllText(string filePath, string content)
        {
            System.IO.File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private bool Exists(string filePath)
        {
            return System.IO.File.Exists(filePath);
        }

        public string ExecuteApplication(
            string pathToExe,
            string workingDirectory,
            string args)
        {
            var processStartInfo = new ProcessStartInfo(pathToExe, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            var output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            this.logger.LogInformation(
                "Output from '" + pathToExe + " " + args + "' was: " + Environment.NewLine + output);

            return output;
        }
    }
}