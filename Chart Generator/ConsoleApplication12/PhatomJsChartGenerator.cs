using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConsoleApplication12
{
    public class PhatomJsChartGenerator : IDisposable
    {
        private readonly object _lock = new object();
        private Process _process;
        private TaskCompletionSource<string> _tcs;

        public void Initialize()
        {
            if (_process != null)
                return;

            lock (_lock)
            {
                if (_process != null)
                    return;

                var psi = new ProcessStartInfo();
                psi.FileName = @"../../phantomjs.exe"; // TODO configuration
                psi.Arguments = "../../script.js"; // TODO embed resource
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                var process = Process.Start(psi);
                Debug.Assert(process != null, "process != null");
                process.OutputDataReceived += _process_OutputDataReceived;
                process.BeginOutputReadLine();
                _process = process;
            }
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            var tcs = _tcs;
            if (tcs != null && e.Data.StartsWith("MC0001:"))
            {
                _tcs = null;
                tcs.SetResult(e.Data.Substring("MC0001:".Length));
            }
        }

        public void SendCommand(string command)
        {
            _process.StandardInput.WriteLine(command);
        }

        public bool IsStarted => _process != null && !_process.HasExited;
        
        public Task GenerateImage(ChartConfiguration chartConfiguration, string imagePath, bool useCache = true)
        {
            if (chartConfiguration == null) throw new ArgumentNullException(nameof(chartConfiguration));
            if (imagePath == null) throw new ArgumentNullException(nameof(imagePath));

            if (useCache)
            {
                FileInfo fi = new FileInfo(imagePath);
                if (fi.Exists && fi.Length > 0)
                    return Task.FromResult(imagePath);
            }

            if (_tcs != null)
                throw new InvalidOperationException("There is already a task in progress");

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            string chartConfig = $@" 
function(){{
    google.charts.setOnLoadCallback(function () {{
        // console.log('setOnLoadCallback');
        var chartId = 'chart_wrapper';
    
        // clear previous chart
        var existingElement = document.getElementById(chartId);
        if (existingElement) {{
            existingElement.parentNode.removeChild(existingElement);
        }}
    
        // Create the chart container
        var div = document.createElement('div');
        div.id = chartId;
        div.style.display='inline';
        div.style.overflow='visible';
        document.body.appendChild(div);

        // Create the data table.
        var data = new google.visualization.DataTable({{cols: {JsonConvert.SerializeObject(chartConfiguration.Columns, settings)}}});
        data.addRows({JsonConvert.SerializeObject(chartConfiguration.Rows, settings)});
    
        // Set chart options
        var options = {{
            'title': {JsonConvert.SerializeObject(chartConfiguration.Title, settings)},
            'width': {JsonConvert.SerializeObject(chartConfiguration.Width, settings)},
            'height': {JsonConvert.SerializeObject(chartConfiguration.Height, settings)}
        }};
    
        var chart = new google.visualization.{chartConfiguration.ChartType}Chart(div);
        chart.draw(data, options);
        div.classList.add('drawn');
    }});
}}
";

            // remove comment and new line
            string command = string.Join(" ", chartConfig.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()).Where(line => !line.StartsWith("//")));
            SendCommand(command);
            SendCommand("renderChart " + chartConfiguration.Width + " " + chartConfiguration.Height + " " + imagePath);
            _tcs = new TaskCompletionSource<string>();
            return _tcs.Task;
        }

        public void Dispose()
        {
            if (_process != null)
            {
                SendCommand("phantom.exit()");
                _process.WaitForExit();
                _process = null;
            }
        }
    }
}