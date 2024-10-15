namespace LuaDependencyFinder
{
    internal class ApplicationContext
    {
        private readonly IDictionary<Type, object> m_dependencies;

        public ApplicationContext()
        {
            m_dependencies = new Dictionary<Type, object>();
        }

        public void AddService<T>(object instance)
        {
            m_dependencies.Add(typeof(T), instance);
        }

        public T GetService<T>()
        {
            if (!m_dependencies.TryGetValue(typeof(T), out var service))
            {
                throw new Exception($"Service of type {typeof(T)} is not available.");
            }

            return (T)service;
        }
    }
}
