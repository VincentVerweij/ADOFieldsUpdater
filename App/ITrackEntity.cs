namespace App
{
    internal interface ITrackEntity
    {
        public string Description { get; init; }
        public DateTimeOffset? Start { get; init; }
        public DateTimeOffset? End { get; init; }
    }
}
