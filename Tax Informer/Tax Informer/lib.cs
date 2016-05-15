using Android.Content;
using Android.Database.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lib
{
    public class StackedQueue<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {
        private List<T> __list = null;

        #region Queue

        public void Enqueue(T value) => __list.Add(value);

        public T Dequeue()
        {
            var obj = default(T);
            lock (__list)
            {
                obj = __list[0];
                __list.RemoveAt(0);
            }
            return obj;
        }

        public T PeekAsQueue() => __list[0];

        #endregion Queue

        #region Stack

        public T Pop()
        {
            T obj = default(T);
            lock (__list)
            {
                obj = __list[__list.Count - 1];
                __list.RemoveAt(__list.Count - 1);
            }
            return obj;
        }

        public T PeekAsStack() => __list[__list.Count - 1];

        public void Push(T value) => __list.Add(value);

        #endregion Stack

        #region List

        public T this[int index]
        {
            get
            {
                return __list[index];
            }

            set
            {
                __list[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return __list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item) => __list.Add(item);

        public void Clear() => __list.Clear();

        public bool Contains(T item) => __list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => __list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => __list.GetEnumerator();

        public int IndexOf(T item) => __list.IndexOf(item);

        public void Insert(int index, T item) => __list.Insert(index, item);

        public bool Remove(T item) => __list.Remove(item);

        public void RemoveAt(int index) => __list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => __list.GetEnumerator();

        #endregion List

        #region Constructor

        public StackedQueue()
        {
            __list = new List<T>();
        }

        public StackedQueue(int capacity)
        {
            __list = new List<T>(capacity);
        }

        public StackedQueue(IEnumerable<T> collection)
        {
            __list = new List<T>(collection);
        }

        #endregion Constructor
    }

    public class DictionaryStackedQueue<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> __dic = null;
        private StackedQueue<TKey> __stackedQueue = null;

        #region Queue

        public void Enqueue(TKey key, TValue value) => Add(key, value);

        public KeyValuePair<TKey, TValue> Dequeue()
        {
            var key = __stackedQueue[0];
            var value = __dic[key];
            Remove(key);
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public KeyValuePair<TKey, TValue> PeekAsQueue()
        {
            var key = __stackedQueue[0];
            var value = __dic[key];
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        #endregion Queue

        #region Stack

        public void Push(TKey key, TValue value) => Add(key, value);

        public KeyValuePair<TKey, TValue> Pop()
        {
            var key = __stackedQueue[__stackedQueue.Count - 1];
            var value = __dic[key];
            Remove(key);
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public KeyValuePair<TKey, TValue> PeekAsStack()
        {
            var key = __stackedQueue[__stackedQueue.Count - 1];
            var value = __dic[key];
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        #endregion Stack

        #region Dictionary

        public TValue this[TKey key]
        {
            get
            {
                return __dic[key];
            }

            set
            {
                __dic[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return __dic.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return __dic.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return __dic.Values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Add(TKey key, TValue value)
        {
            __dic.Add(key, value);
            __stackedQueue.Add(key);
        }

        public void Clear()
        {
            __dic.Clear();
            __stackedQueue.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => __dic.Contains(item);

        public bool ContainsKey(TKey key) => __dic.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => __dic.GetEnumerator();

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public bool Remove(TKey key)
        {
            if (__dic.Remove(key))
                return __stackedQueue.Remove(key);
            else return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => __dic.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => __dic.GetEnumerator();

        #endregion Dictionary

        #region Constructor

        public DictionaryStackedQueue()
        {
            __dic = new Dictionary<TKey, TValue>();
            __stackedQueue = new StackedQueue<TKey>();
        }

        public DictionaryStackedQueue(int capacity)
        {
            __dic = new Dictionary<TKey, TValue>(capacity);
            __stackedQueue = new StackedQueue<TKey>(capacity);
        }

        public DictionaryStackedQueue(IDictionary<TKey, TValue> dictionary)
        {
            __dic = new Dictionary<TKey, TValue>(dictionary);
            __stackedQueue = new StackedQueue<TKey>();
        }

        #endregion Constructor
    }

    #region MySQLite

    public class MySQLite
    {
        public Context Context { get; } = null;
        public string DatabaseName { get; } = null;
        public SQLiteDatabase UnderlayingDatabase { get; } = null;
        public string DatabaseFilePath { get { return Context?.GetDatabasePath(DatabaseFilePath)?.AbsolutePath; } }

        public MySQLite(Context context, string databaseName)
        {
            this.Context = context;
            this.DatabaseName = databaseName;
            UnderlayingDatabase = context.OpenOrCreateDatabase(databaseName, FileCreationMode.Private, null);
        }

        public void CreateTable<T>(string tableName) => CreateTable(tableName, new MySQLiteTableMap(typeof(T)));

        public void CreateTable(string tableName, MySQLiteTableMap tableMap) => CreateTable(tableName, tableMap.ToSql());

        public void CreateTable(string tableName, string tableMap)
        {
            if (UnderlayingDatabase == null) throw new InvalidOperationException("The Database is not created.");
            UnderlayingDatabase.ExecSQL($"CREATE TABLE {tableName}({tableMap});");
        }

        public void InsertRow<T>(string tableName, T objInstance)
            => InsertRow(tableName, new MySQLiteTableMap(objInstance.GetType()).GetColumns(), createValues(objInstance));

        public void InsertRow(string tableName, string columnNames, string values)
        {
            if (UnderlayingDatabase == null) throw new InvalidOperationException("The Database is not created.");
            UnderlayingDatabase.ExecSQL($"INSERT INTO {tableName} ({columnNames}) VALUES ({values});");
        }

        private string createValues(object obj)
        {
            //TODO: create values from object using Reflection
            //var pro = objInstance.GetType().GetProperties()[0];
            //var  value = pro.GetValue(objInstance);
            return string.Empty;
        }

        public List<T> GetItem<T>(string tableName, string predicate = "")
        {
            var columns = new MySQLiteTableMap(typeof(T)).GetColumns();
            var cursor = GetItem(tableName, columns, predicate);

            if (cursor == null || cursor.Count == 0) return null;
            //TODO: Implement
            return null;
        }

        public SQLiteCursor GetItem(string tableName, string command, string predicate = "")
        {
            if (UnderlayingDatabase == null) throw new InvalidOperationException("The Database is not created.");
            return (SQLiteCursor)UnderlayingDatabase.RawQuery($"SELECT {command} FROM {tableName}"
                        + predicate == string.Empty ? ";" : ($" WHERE {predicate};"), null);
        }
    }

    public class MySQLiteTableMap
    {
        public List<MySQLiteObjectMap> objectMapCollection { get; } = null;

        public MySQLiteTableMap()
        {
            objectMapCollection = new List<MySQLiteObjectMap>();
        }

        public MySQLiteTableMap(Type type)
        {
            //TODO: Implement MySQLiteTableMap from Type
        }

        public string ToSql()
        {
            if (objectMapCollection.Count == 0) return string.Empty;

            string sql = string.Empty;
            foreach (var objMap in objectMapCollection)
                sql += objMap.ToSql() + ",";
            return sql.Substring(0, sql.Length - 1);
        }

        public string GetColumns()
        {
            if (objectMapCollection.Count == 0) return string.Empty;

            string columns = string.Empty;
            foreach (var objMap in objectMapCollection)
                columns += objMap.Name + ",";
            return columns.Substring(0, columns.Length - 1);
        }
    }

    public class MySQLiteObjectMap
    {
        public string Name { get; set; } = null;
        public DataTypeInfo DataType { get; set; } = DataTypeInfo.NULL;

        public MySQLiteObjectMap(string name, DataTypeInfo dataType)
        {
            this.Name = name;
            this.DataType = dataType;
        }

        public MySQLiteObjectMap(PropertyInfo propertyInfo)
        {
            this.Name = propertyInfo.Name;
            //TODO:Implement the MySQLiteObjectMap with PropertyInfo
        }

        public string ToSql() => $"{Name} {DataType.ToString()}";

        public enum DataTypeInfo
        {
            NULL,
            INT,
            TEXT,
            DOUBLE,
            FLOAT,
            BOOLEAN
        }
    }

    #endregion MySQLite
}