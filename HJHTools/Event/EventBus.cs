namespace HJHTools.Event
{
    #pragma warning disable CS8602 // 解引用可能出现空引用。
    public static class EventBus
    {
        private static IEventAggregator? _eventAggregator;

        public static void Init(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        private static Dictionary<SubscriptionToken, Type> TokenMap { get; } = new();


        private static void EnsureInitialized() {
            if (_eventAggregator == null)
            {
                throw new InvalidOperationException("EventBus is not initialized, Please call Init method first.");
            }
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public static SubscriptionToken Subscribe<T>(Action<T> action) {
            EnsureInitialized();
            var subscriptionToken = _eventAggregator.GetEvent<PubSubEvent<T>>().Subscribe(action);
            TokenMap.Add(subscriptionToken, typeof(PubSubEvent<T>));
            return subscriptionToken;
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public static void Publish<T>(T result) {
            EnsureInitialized();
            _eventAggregator.GetEvent<PubSubEvent<T>>().Publish(result);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public static void Unsubscribe(SubscriptionToken token) {
            EnsureInitialized();

            if (!TokenMap.TryGetValue(token, out var eventType)) return;
            // 使用反射获取泛型方法 GetEvent<T> 的定义
            var getEventMethod = typeof(IEventAggregator).GetMethod("GetEvent")?.MakeGenericMethod(eventType);

            // 获取具体的事件对象
            var eventInstance = getEventMethod?.Invoke(_eventAggregator, null);

            // 获取事件的 Unsubscribe 方法，明确指定参数类型为 SubscriptionToken
            var unsubscribeMethod =
                eventInstance?.GetType().GetMethod("Unsubscribe", new[] { typeof(SubscriptionToken) });

            // 取消订阅
            unsubscribeMethod?.Invoke(eventInstance, new object[] { token });
            TokenMap.Remove(token);
        }

        /**
         * 移除所有订阅
         */
        public static void UnAllSubscriptions() {
            EnsureInitialized();
            foreach (var (key, _) in TokenMap)
            {
                Unsubscribe(key);
            }
        }
    }
}