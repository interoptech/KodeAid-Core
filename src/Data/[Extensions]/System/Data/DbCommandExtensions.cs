﻿// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KodeAid;

namespace System.Data
{
    public static class DbCommandExtensions
    {
        private static readonly IEnumerable<IDataParameter> _emptyParameters = Enumerable.Empty<IDataParameter>();

        public static IEnumerable<IDataParameter> AutoGenerateParameters(this IDbCommand command, IEnumerable parameterValues, string prefix = "@p")
        {
            ArgCheck.NotNullOrEmpty("prefix", prefix);
            if (parameterValues == null)
                return _emptyParameters;
            var typedValues = parameterValues.Cast<object>().ToList();
            if (!typedValues.Any())
                return _emptyParameters;
            return typedValues.Select((p, i) =>
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = prefix + i;
                parameter.Value = p ?? DBNull.Value;
                return parameter;
            }).ToList();
        }

        public static int ExecuteNonQuery(this IDbCommand command, string commandText)
        {
            return ExecuteNonQuery(command, commandText, (IEnumerable)null);
        }

        public static int ExecuteNonQuery(this IDbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteNonQuery(command, commandText, (IEnumerable)parameterValues);
        }

        public static int ExecuteNonQuery(this IDbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteNonQuery(command, commandText, AutoGenerateParameters(command, parameterValues));
        }

        public static int ExecuteNonQuery(this IDbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteNonQuery(command, commandText, (IEnumerable<IDataParameter>)parameters);
        }

        public static int ExecuteNonQuery(this IDbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            var shouldCloseConnection = false;
            try
            {
                shouldCloseConnection = PrepareCommandAndEnsureConnectionIsOpen(command, commandText, parameters);
                return command.ExecuteNonQuery();
            }
            finally
            {
                if (shouldCloseConnection)
                    command.Connection.Close();
            }
        }

        public static TScalar ExecuteScalar<TScalar>(this IDbCommand command, string commandText)
        {
            return ExecuteScalar<TScalar>(command, commandText, (IEnumerable)null);
        }

        public static TScalar ExecuteScalar<TScalar>(this IDbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteScalar<TScalar>(command, commandText, (IEnumerable)parameterValues);
        }

        public static TScalar ExecuteScalar<TScalar>(this IDbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteScalar<TScalar>(command, commandText, AutoGenerateParameters(command, parameterValues));
        }

        public static TScalar ExecuteScalar<TScalar>(this IDbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteScalar<TScalar>(command, commandText, (IEnumerable<IDataParameter>)parameters);
        }

        public static TScalar ExecuteScalar<TScalar>(this IDbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            return (TScalar)ExecuteScalar(command, commandText, parameters);
        }

        public static object ExecuteScalar(this IDbCommand command, string commandText)
        {
            return ExecuteScalar(command, commandText, (IEnumerable)null);
        }

        public static object ExecuteScalar(this IDbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteScalar(command, commandText, (IEnumerable)parameterValues);
        }

        public static object ExecuteScalar(this IDbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteScalar(command, commandText, AutoGenerateParameters(command, parameterValues));
        }

        public static object ExecuteScalar(this IDbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteScalar(command, commandText, (IEnumerable<IDataParameter>)parameters);
        }

        public static object ExecuteScalar(this IDbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            var shouldCloseConnection = false;
            try
            {
                shouldCloseConnection = PrepareCommandAndEnsureConnectionIsOpen(command, commandText, parameters);
                var result = command.ExecuteScalar();
                if (result == DBNull.Value)
                    return null;
                return result;
            }
            finally
            {
                if (shouldCloseConnection)
                    command.Connection.Close();
            }
        }

        public static IDataReader ExecuteReader(this IDbCommand command, string commandText)
        {
            return ExecuteReader(command, commandText, (IEnumerable)null);
        }

        public static IDataReader ExecuteReader(this IDbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteReader(command, commandText, (IEnumerable)parameterValues);
        }

        public static IDataReader ExecuteReader(this IDbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteReader(command, commandText, AutoGenerateParameters(command, parameterValues));
        }

        public static IDataReader ExecuteReader(this IDbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteReader(command, commandText, (IEnumerable<IDataParameter>)parameters);
        }

        public static IDataReader ExecuteReader(this IDbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            var shouldCloseConnection = PrepareCommandAndEnsureConnectionIsOpen(command, commandText, parameters);
            return command.ExecuteReader(shouldCloseConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default);
        }

        public static bool TestConnection(this IDbCommand command)
        {
            return command.ExecuteScalar<int>($@"SELECT 1") == 1;
        }

        public static bool DoesTableExist(this IDbCommand command, string table, string schema = "dbo")
        {
            return DoesObjectExist(command, table, "U", schema);
        }

        public static bool DoesTriggerExistAsync(this IDbCommand command, string trigger, string schema = "dbo")
        {
            return DoesObjectExist(command, trigger, "TR", schema);
        }

        public static bool DoesObjectExist(this IDbCommand command, string name, string type, string schema = "dbo")
        {
            ArgCheck.NotNull(nameof(command), command);
            if (command.GetType() != typeof(SqlCommand))
                throw new NotSupportedException("Method DoesObjectExistAsync currently only supports SQL server.");
            return command.ExecuteScalar<int>($@"IF OBJECT_ID('[{schema}].[{name}]', '{type}') IS NOT NULL SELECT 1 ELSE SELECT 0") == 1;
        }

        public static void AddParameter(this IDbCommand command, string name, object value, DbType type)
        {
            ArgCheck.NotNull(nameof(command), command);
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = type;
        }

        public static void AddParameter(this IDbCommand command, string name, object value, DbType type, int size)
        {
            ArgCheck.NotNull(nameof(command), command);
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = type;
            parameter.Size = size;
            command.Parameters.Add(parameter);
        }

        public static void AddParameter(this IDbCommand command, string name, object value, DbType type, int scale, int precision)
        {
            ArgCheck.NotNull(nameof(command), command);
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = type;
            parameter.Scale = (byte)scale;
            parameter.Precision = (byte)precision;
            command.Parameters.Add(parameter);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText)
        {
            return ExecuteNonQueryAsync(command, commandText, CancellationToken.None);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, CancellationToken cancellationToken)
        {
            return ExecuteNonQueryAsync(command, commandText, (IEnumerable)null, cancellationToken);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteNonQueryAsync(command, commandText, CancellationToken.None, parameterValues);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params object[] parameterValues)
        {
            return ExecuteNonQueryAsync(command, commandText, parameterValues, cancellationToken);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteNonQueryAsync(command, commandText, parameterValues, CancellationToken.None);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, IEnumerable parameterValues, CancellationToken cancellationToken)
        {
            return ExecuteNonQueryAsync(command, commandText, command.AutoGenerateParameters(parameterValues), cancellationToken);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteNonQueryAsync(command, commandText, CancellationToken.None, parameters);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params IDataParameter[] parameters)
        {
            return ExecuteNonQueryAsync(command, commandText, parameters, cancellationToken);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            return ExecuteNonQueryAsync(command, commandText, parameters, CancellationToken.None);
        }

        public static async Task<int> ExecuteNonQueryAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters, CancellationToken cancellationToken)
        {
            var shouldCloseConnection = false;
            try
            {
                shouldCloseConnection = await PrepareCommandAndEnsureConnectionIsOpenAsync(command, commandText, parameters, cancellationToken).ConfigureAwait(false);
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (shouldCloseConnection)
                    command.Connection.Close();
            }
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, CancellationToken.None);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, CancellationToken cancellationToken)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, (IEnumerable)null, cancellationToken);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, CancellationToken.None, parameterValues);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, CancellationToken cancellationToken, params object[] parameterValues)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, parameterValues, cancellationToken);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, parameterValues, CancellationToken.None);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, IEnumerable parameterValues, CancellationToken cancellationToken)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, command.AutoGenerateParameters(parameterValues), cancellationToken);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, CancellationToken.None, parameters);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, CancellationToken cancellationToken, params IDataParameter[] parameters)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, parameters, cancellationToken);
        }

        public static Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            return ExecuteScalarAsync<TScalar>(command, commandText, parameters, CancellationToken.None);
        }

        public static async Task<TScalar> ExecuteScalarAsync<TScalar>(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters, CancellationToken cancellationToken)
        {
            return (TScalar)await ExecuteScalarAsync(command, commandText, parameters, cancellationToken).ConfigureAwait(false);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText)
        {
            return ExecuteScalarAsync(command, commandText, CancellationToken.None);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, CancellationToken cancellationToken)
        {
            return ExecuteScalarAsync(command, commandText, (IEnumerable)null, cancellationToken);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteScalarAsync(command, commandText, CancellationToken.None, parameterValues);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params object[] parameterValues)
        {
            return ExecuteScalarAsync(command, commandText, parameterValues, cancellationToken);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteScalarAsync(command, commandText, parameterValues, CancellationToken.None);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, IEnumerable parameterValues, CancellationToken cancellationToken)
        {
            return ExecuteScalarAsync(command, commandText, command.AutoGenerateParameters(parameterValues), cancellationToken);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteScalarAsync(command, commandText, CancellationToken.None, parameters);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params IDataParameter[] parameters)
        {
            return ExecuteScalarAsync(command, commandText, parameters, cancellationToken);
        }

        public static Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            return ExecuteScalarAsync(command, commandText, parameters, CancellationToken.None);
        }

        public static async Task<object> ExecuteScalarAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters, CancellationToken cancellationToken)
        {
            var shouldCloseConnection = false;
            try
            {
                shouldCloseConnection = await PrepareCommandAndEnsureConnectionIsOpenAsync(command, commandText, parameters, cancellationToken).ConfigureAwait(false);
                var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result == DBNull.Value)
                    return null;
                return result;
            }
            finally
            {
                if (shouldCloseConnection)
                    command.Connection.Close();
            }
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText)
        {
            return ExecuteReaderAsync(command, commandText, CancellationToken.None);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, CancellationToken cancellationToken)
        {
            return ExecuteReaderAsync(command, commandText, (IEnumerable)null, cancellationToken);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, params object[] parameterValues)
        {
            return ExecuteReaderAsync(command, commandText, CancellationToken.None, parameterValues);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params object[] parameterValues)
        {
            return ExecuteReaderAsync(command, commandText, parameterValues, cancellationToken);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, IEnumerable parameterValues)
        {
            return ExecuteReaderAsync(command, commandText, parameterValues, CancellationToken.None);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, IEnumerable parameterValues, CancellationToken cancellationToken)
        {
            return ExecuteReaderAsync(command, commandText, command.AutoGenerateParameters(parameterValues), cancellationToken);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, params IDataParameter[] parameters)
        {
            return ExecuteReaderAsync(command, commandText, CancellationToken.None, parameters);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, CancellationToken cancellationToken, params IDataParameter[] parameters)
        {
            return ExecuteReaderAsync(command, commandText, parameters, cancellationToken);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            return ExecuteReaderAsync(command, commandText, parameters, CancellationToken.None);
        }

        public static async Task<DbDataReader> ExecuteReaderAsync(this DbCommand command, string commandText, IEnumerable<IDataParameter> parameters, CancellationToken cancellationToken)
        {
            var shouldCloseConnection = await PrepareCommandAndEnsureConnectionIsOpenAsync(command, commandText, parameters, cancellationToken).ConfigureAwait(false);
            return await command.ExecuteReaderAsync(shouldCloseConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<bool> TestConnectionAsync(this DbCommand command)
        {
            return (await command.ExecuteScalarAsync<int>($@"SELECT 1").ConfigureAwait(false)) == 1;
        }

        public static Task<bool> DoesTableExistAsync(this DbCommand command, string table, string schema = "dbo")
        {
            return DoesObjectExistAsync(command, table, "U", schema);
        }

        public static Task<bool> DoesTriggerExistAsync(this DbCommand command, string trigger, string schema = "dbo")
        {
            return DoesObjectExistAsync(command, trigger, "TR", schema);
        }

        public static async Task<bool> DoesObjectExistAsync(this DbCommand command, string name, string type, string schema = "dbo")
        {
            ArgCheck.NotNull(nameof(command), command);
            if (command.GetType() != typeof(SqlCommand))
                throw new NotSupportedException("Method DoesObjectExistAsync currently only supports SQL server.");
            return (await command.ExecuteScalarAsync<int>($@"IF OBJECT_ID('[{schema}].[{name}]', '{type}') IS NOT NULL SELECT 1 ELSE SELECT 0").ConfigureAwait(false)) == 1;
        }

        /// <summary>
        /// Prepares the command for execution and returns true if the connection should be closed within the extension method; otherwise false.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns>True if the connection should be closed within the extension method; otherwise false.</returns>
        private static bool PrepareCommandAndEnsureConnectionIsOpen(IDbCommand command, string commandText, IEnumerable<IDataParameter> parameters)
        {
            ArgCheck.NotNull("command", command);
            ArgCheck.NotNullOrEmpty("commandText", commandText);
            command.CommandText = commandText;
            if (parameters != null && parameters.Any())
            {
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);
            }

            if (command.Connection.State == ConnectionState.Closed)
            {
                command.Connection.Open();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prepares the command for execution and returns true if the connection should be closed within the extension method; otherwise false.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns>True if the connection should be closed within the extension method; otherwise false.</returns>
        private static async Task<bool> PrepareCommandAndEnsureConnectionIsOpenAsync(DbCommand command, string commandText, IEnumerable<IDataParameter> parameters, CancellationToken cancellationToken)
        {
            ArgCheck.NotNull("command", command);
            ArgCheck.NotNullOrEmpty("commandText", commandText);
            command.CommandText = commandText;
            if (parameters != null && parameters.Any())
            {
                foreach (var parameter in parameters)
                    command.Parameters.Add(parameter);
            }

            if (command.Connection.State == ConnectionState.Closed)
            {
                await command.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}
