using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MultiSuika.Ball;
using MultiSuika.Utilities;

namespace MultiSuika.Container
{
    public class ContainerTracker : ItemTracker<ContainerInstance, ContainerTrackerInformation>
    {
        #region Singleton

        public static ContainerTracker Instance => _instance ??= new ContainerTracker();

        private static ContainerTracker _instance;

        private ContainerTracker()
        {
        }

        private void Awake()
        {
            _instance = this;
        }

        #endregion

        public ActionMethodPlayerWrapper<(BallInstance, ContainerInstance)> OnContainerHit { get; } =
            new ActionMethodPlayerWrapper<(BallInstance, ContainerInstance)>();

        protected override ContainerTrackerInformation CreateInformationInstance(ContainerInstance item,
            List<int> playerIndex)
        {
            return new ContainerTrackerInformation(item, playerIndex);
        }
    }

    public class ContainerTrackerInformation : ItemInformation<ContainerInstance>
    {
        public ActionMethodPlayerWrapper<(BallInstance, ContainerInstance)> OnContainerHit { get; } =
            new ActionMethodPlayerWrapper<(BallInstance, ContainerInstance)>();

        public ContainerTrackerInformation(ContainerInstance item, List<int> playerIndex) : base(item, playerIndex)
        {
        }
    }
}