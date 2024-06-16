using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MinoQueue
{
    public List<MinoType> MinoQueueList { get; private set; } = new List<MinoType>();
    public MinoType[] RandomizeMino()
    {
        MinoType[] rentMinoQueue =
        {
            MinoType.MinoT,
            MinoType.MinoS,
            MinoType.MinoZ,
            MinoType.MinoL,
            MinoType.MinoJ,
            MinoType.MinoO,
            MinoType.MinoI,
        };
        return rentMinoQueue.OrderBy(i => Guid.NewGuid()).ToArray();
    }
    public MinoType GetNextMino()
    {
        if (MinoQueueList.Count <= 7)
        {
            MinoQueueList.AddRange(RandomizeMino());
        }
        var retNextMino = MinoQueueList[0];
        MinoQueueList.RemoveAt(0);
        return retNextMino;
    }
}