namespace App
{
    internal interface IBindingPart
    {
        static string BindingPlaceholder { get; }

        public string ConnectionNameToBind { get; init; }
        public int ConnectionItemIdToUpdate { get; init; }
    }
}
