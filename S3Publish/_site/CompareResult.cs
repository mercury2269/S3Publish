namespace S3Publish
{
    public enum CompareStatus
    {
        Retained, 
        Added,
        Modified,
        Deleted,

    }

    public class CompareResult
    {
        public string Key { set; get; }                                                
        public CompareStatus Status { get; set; }

        public CompareResult(string key, CompareStatus status)
        {
            Key = key;
            Status = status;
        }

        protected bool Equals(CompareResult other)
        {
            return string.Equals(Key, other.Key) && Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CompareResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ (int) Status;
            }
        }
    }
}