// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Diagnostics
{
#if !SILVERLIGHT
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Security;

	public class PerformanceMetricsFactory : IPerformanceMetricsFactory
	{
		private const string CastleWindsorCategoryName = "Castle Windsor";
		private const string InstanesTrackedByTheReleasePolicyCounterName = "Instances tracked by the release policy";

		public PerformanceMetricsFactory()
		{
			Initialize();
		}

		public bool InitializedSuccessfully { get; private set; }

		public ITrackedComponentsPerformanceCounter CreateInstancesTrackedByReleasePolicyCounter(string name)
		{
			var counter = BuildInstancesTrackedByReleasePolicyCounter(name);
			if (counter == null)
			{
				return NullPerformanceCounter.Instance;
			}
			return new TrackedComponentsPerformanceCounterWrapper(counter);
		}

		private PerformanceCounter BuildInstancesTrackedByReleasePolicyCounter(string name)
		{
			if (InitializedSuccessfully == false)
			{
				return null;
			}
			try
			{
				return new PerformanceCounter(CastleWindsorCategoryName,
				                              InstanesTrackedByTheReleasePolicyCounterName,
				                              name,
				                              readOnly: false) { RawValue = 0L };
			}
				// exception types we should expect according to http://msdn.microsoft.com/en-us/library/356cx381.aspx
			catch (Win32Exception)
			{
			}
			catch (PlatformNotSupportedException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return null;
		}

		private static void CreateWindsorCategoryAndCounters()
		{
			PerformanceCounterCategory.Create(CastleWindsorCategoryName,
			                                  "Performance counters published by the Castle Windsor container",
			                                  PerformanceCounterCategoryType.MultiInstance,
			                                  new CounterCreationDataCollection
			                                  {
			                                  	new CounterCreationData
			                                  	{
			                                  		CounterType = PerformanceCounterType.NumberOfItems32,
			                                  		CounterName = InstanesTrackedByTheReleasePolicyCounterName,
			                                  		CounterHelp = "List of instances tracked by the release policy in the container. " +
			                                  		              "Notice that does not include all alive objects tracked by the container, just the ones tracked by the policy."
			                                  	}
			                                  });
		}

		private void Initialize()
		{
			try
			{
				if (PerformanceCounterCategory.Exists(CastleWindsorCategoryName))
				{
					var categories = PerformanceCounterCategory.GetCategories();
					InitializedSuccessfully = categories.SingleOrDefault(c => c.CategoryName == CastleWindsorCategoryName) != null;
				}
				else
				{
					CreateWindsorCategoryAndCounters();
				}
			}
			catch (Win32Exception)
			{
				InitializedSuccessfully = false;
			}
			catch (UnauthorizedAccessException)
			{
				InitializedSuccessfully = false;
			}
				// it's not in the documentation but PerformanceCounterCategory.Create can also throw SecurityException,
			catch (SecurityException)
			{
				InitializedSuccessfully = false;
			}
		}
	}
#endif
}