using System;
using Oracle.ManagedDataAccess.Types;
using Oracle.ManagedDataAccess.Client;

namespace NetTopologySuite.IO.UdtBase
{
    public abstract class OracleCustomTypeBase<T> : INullable, IOracleCustomType, IOracleCustomTypeFactory
        where T : OracleCustomTypeBase<T>, new()
    {
        private static readonly string ErrorMessageHead = "Error converting Oracle User Defined Type to .Net Type " +
                                                          typeof(T) +
                                                          ", oracle column is null, failed to map to . NET valuetype, column ";

        private OracleConnection _connection;
        private object _pObject;

        private bool _isNull;

        public virtual bool IsNull => _isNull;

        public static T Null => new T { _isNull = true };

        public IOracleCustomType CreateObject() => new T();

        protected void SetConnectionAndPointer(OracleConnection connection, object pObject)
        {
            _connection = connection;
            _pObject = pObject;
        }

        public abstract void MapFromCustomObject();
        public abstract void MapToCustomObject();

        public void FromCustomObject(OracleConnection con, object pObject)
        {
            SetConnectionAndPointer(con, pObject);
            MapFromCustomObject();
        }

        public void ToCustomObject(OracleConnection con, object pObject)
        {
            SetConnectionAndPointer(con, pObject);
            MapToCustomObject();
        }

        protected void SetValue(string oracleColumnName, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pObject, oracleColumnName, value);
            }
        }

        protected void SetValue(int oracleColumnId, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pObject, oracleColumnId, value);
            }
        }

        protected TUser GetValue<TUser>(string oracleColumnName)
        {
            if (OracleUdt.IsDBNull(_connection, _pObject, oracleColumnName))
            {
                if (default(TUser) != null)
                {
                    throw new Exception(ErrorMessageHead + oracleColumnName + " of value type " +
                                        typeof(TUser));
                }

                return default(TUser);
            }

            return (TUser)OracleUdt.GetValue(_connection, _pObject, oracleColumnName);
        }

        protected TUser GetValue<TUser>(int oracleColumnId)
        {
            if (OracleUdt.IsDBNull(_connection, _pObject, oracleColumnId))
            {
                if (default(TUser) != null)
                {
                    throw new Exception(ErrorMessageHead + oracleColumnId.ToString() + " of value type " +
                                        typeof(TUser));
                }

                return default(TUser);
            }

            return (TUser)OracleUdt.GetValue(_connection, _pObject, oracleColumnId);
        }
    }
}
