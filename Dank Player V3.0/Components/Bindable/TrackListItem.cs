namespace Dank_Player_V3._0.Components.Bindable
{
    class TrackListItem
    {
        public string title { get; set; }
        public string duration { get; set; }
        public string path { get; set; }

        public TrackListItem(string title, string duration, string path)
        {
            this.title = title;
            this.duration = duration;
            this.path = path;
        }
    }
}
