using System.Text.Json;
using YYHEggEgg.Utils;

namespace HappyGenyuanImsactUpdate
{
    internal class Patch
    {
        public Patch(DirectoryInfo datadir, string path7z, string pathHdiff)
        {
            Path7z = path7z;
            PathHdiff = pathHdiff;
            this.datadir = datadir;
        }

        public string Path7z { get; set; }
        public string PathHdiff { get; set; }
        public DirectoryInfo datadir { get; set; }

        #region Patch hdiff
        /// <summary>
        /// Patch hdiff
        /// </summary>
        /// <returns>hdiff files used for deleteing</returns>
        public async Task Hdiff()
        {
            var hdiffs = new List<string>();
            var invokes = new List<OuterInvokeInfo>();

            var hdifftxtPath = $"{datadir}\\hdifffiles.txt";
            var hdiffjsonPath = $"{datadir}\\hdiffmap.json";
            if (File.Exists(hdifftxtPath))
            {
                using (StreamReader hdiffreader = new(hdifftxtPath))
                {
                    while (true)
                    {
                        string? output = hdiffreader.ReadLine();
                        if (output == null) break;
                        else
                        {
                            var doc = JsonDocument.Parse(output);
                            //{"remoteName": "name.pck"}
                            string hdiffName = datadir.FullName + '/'
                                + doc.RootElement.GetProperty("remoteName").GetString();
                            //command:  -f (original file) (patch file)   (output file)
                            //  hpatchz -f name.pck        name.pck.hdiff name.pck
                            string hdiffPathstd = new FileInfo(hdiffName).FullName;
                            // If package is created by an individual, he may include
                            // unnecessary files like cache and live updates,
                            // So it's essential to skip some files that doesn't exist.
                            if (!File.Exists(hdiffPathstd)) continue;

                            invokes.Add(new OuterInvokeInfo
                            {
                                ProcessPath = PathHdiff,
                                CmdLine = $"-f \"{hdiffName}\" \"{hdiffName}.hdiff\" \"{hdiffName}\"",
                                AutoTerminateReason = $"hdiff patch for \"{hdiffName}\" failed."
                            });
                            hdiffs.Add($"{hdiffPathstd}.hdiff");
                        }
                    }
                }

                File.Delete(hdifftxtPath);
            } else if (File.Exists(hdiffjsonPath))
            {
                using (StreamReader hdiffreader = new(hdiffjsonPath))
                {
                    //{"source_file_name":"","source_file_md5":"","source_file_size":0,"target_file_name":"","target_file_md5":"","target_file_size":0,"patch_file_name":"","patch_file_md5":"","patch_file_size":0}
                    HdiffMap? json = JsonSerializer.Deserialize<HdiffMap>(hdiffreader.ReadToEnd());
                    if (json == null) return;

                    foreach (var file in json.diff_map)
                    {
                        string sourceName = datadir.FullName + '/' + file.source_file_name;
                        //command:  -f (original file) (patch file)   (output file)
                        //  hpatchz -f name.pck        name.pck.hdiff name.pck
                        string sourcePathstd = new FileInfo(sourceName).FullName;
                        // If package is created by an individual, he may include
                        // unnecessary files like cache and live updates,
                        // So it's essential to skip some files that doesn't exist.
                        if (!File.Exists(sourcePathstd)) continue;

                        string hdiffName = datadir.FullName + '/' + file.patch_file_name;
                        string hdiffPathstd = new FileInfo(sourceName).FullName;

                        string targetName = datadir.FullName + '/' + file.target_file_name;

                        invokes.Add(new OuterInvokeInfo
                        {
                            ProcessPath = PathHdiff,
                            CmdLine = $"-f \"{sourceName}\" \"{hdiffName}\" \"{targetName}\"",
                            AutoTerminateReason = $"hdiff patch for \"{hdiffName}\" failed."
                        });
                        hdiffs.Add(sourcePathstd);
                        hdiffs.Add(hdiffPathstd);
                    }
                }
            }

            await OuterInvoke.RunMultiple(invokes, 3851, 2);

            // Delete .hdiff afterwards
            foreach (var hdiffFile in hdiffs)
            {
                File.Delete($"{hdiffFile}");
            }
        }
        #endregion

        #region Delete Files
        /// <summary>
        /// Process deletedFiles.txt. Notice that files that failed to be deleted will be returned.
        /// </summary>
        /// <returns>files failed to be deleted</returns>
        public List<string> DeleteFiles()
        {
            var delete_delays = new List<string>();

            var deletetxtPath = $"{datadir}\\deletefiles.txt";
            if (File.Exists(deletetxtPath))
            {
                using (StreamReader hdiffreader = new(deletetxtPath))
                {
                    while (true)
                    {
                        string? output = hdiffreader.ReadLine();
                        if (output == null) break;
                        else
                        {
                            string deletedName = datadir.FullName + '\\' + output;
                            if (File.Exists(deletedName))
                                File.Delete(deletedName);
                            else delete_delays.Add(deletedName);
                        }
                    }
                }

                File.Delete(deletetxtPath);
            }

            return delete_delays;
        }
        #endregion
    }
}
