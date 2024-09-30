namespace HappyGenyuanImsactUpdate
{
    public class HdiffMap
    {
        public DiffMap[] diff_map { get; set; }
    }

    public class DiffMap
    {
        public string source_file_name { get; set; }
        public string source_file_md5 { get; set; }
        public int source_file_size { get; set; }
        public string target_file_name { get; set; }
        public string target_file_md5 { get; set; }
        public int target_file_size { get; set; }
        public string patch_file_name { get; set; }
        public string patch_file_md5 { get; set; }
        public int patch_file_size { get; set; }
    }
}
