namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// A generic class supply commonly used CRUD operations.
    /// </summary>
    public interface IDBRepository<T>
    {
        /// <summary>
        /// Adds a <T> enitity to the <T> table.
        /// </summary>
        /// <param name="tObject">Represents a <T> object</param>
        /// <returns>An object of the specied <T> type</returns>
        public T Add(T tObject);

        /// <summary>
        /// Updates a <T> enitity in the <T> table.
        /// </summary>
        /// <param name="tObject">Represents a <T> object</param>
        /// <returns>An object of the specied <T> type</returns>
        public T Update(T tObject);

        /// <summary>
        /// Deletes a <T> enitity from the <T> table.
        /// </summary>
        /// <param name="tObject">Represents a <T> object</param>
        /// <returns>An object of the specied <T> type</returns>
        public bool Delete(T tObject);
    }
}
