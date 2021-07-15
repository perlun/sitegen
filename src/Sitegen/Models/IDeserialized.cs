namespace Sitegen.Models
{
    public interface IDeserialized
    {
        /// <summary>
        /// Event which will be called after deserialization of the object (graph) is complete.
        /// </summary>
        void OnDeserialized();
    }
}
