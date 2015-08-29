﻿// This file is part of Hangfire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Storage;

namespace Hangfire.Server
{
    /// <summary>
    /// Provides information about the context in which the job
    /// is being performed.
    /// </summary>
    public class PerformContext : WorkerContext
    {
        internal PerformContext([NotNull] PerformContext context)
            : this(context, context.Connection, context.BackgroundJob, context.CancellationToken)
        {
            Items = context.Items;
        }

        internal PerformContext(
            [NotNull] WorkerContext workerContext,
            [NotNull] IStorageConnection connection, 
            [NotNull] BackgroundJob backgroundJob,
            [NotNull] IJobCancellationToken cancellationToken)
            : base(workerContext)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (backgroundJob == null) throw new ArgumentNullException("backgroundJob");
            if (cancellationToken == null) throw new ArgumentNullException("cancellationToken");

            Connection = connection;
            BackgroundJob = backgroundJob;
            CancellationToken = cancellationToken;

            Items = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets an instance of the key-value storage. You can use it
        /// to pass additional information between different client filters
        /// or just between different methods.
        /// </summary>
        [NotNull]
        public IDictionary<string, object> Items { get; private set; }

        [NotNull]
        public BackgroundJob BackgroundJob { get; private set; }

        [Obsolete("Please use BackgroundJob property instead. Will be removed in 2.0.0.")]
        public string JobId { get { return BackgroundJob.Id; } }
        [Obsolete("Please use BackgroundJob property instead. Will be removed in 2.0.0.")]
        public Job Job { get { return BackgroundJob.Job; } }
        [Obsolete("Please use BackgroundJob property instead. Will be removed in 2.0.0.")]
        public DateTime CreatedAt { get { return BackgroundJob.CreatedAt; } }

        [NotNull]
        public IJobCancellationToken CancellationToken { get; private set; }

        [NotNull]
        public IStorageConnection Connection { get; private set; }
        
        public void SetJobParameter(string name, object value)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            Connection.SetJobParameter(BackgroundJob.Id, name, JobHelper.ToJson(value));
        }

        public T GetJobParameter<T>(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            try
            {
                return JobHelper.FromJson<T>(Connection.GetJobParameter(BackgroundJob.Id, name));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format(
                    "Could not get a value of the job parameter `{0}`. See inner exception for details.",
                    name), ex);
            }
        }
    }
}
