namespace App
{
    internal interface ITimeTracker
    {
        public Task<List<ITrackEntity>> GetTimeTrackingEntityAsync(int daysAgo = -1);
    }
}
