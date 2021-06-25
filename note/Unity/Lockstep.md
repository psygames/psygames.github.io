## 文章

- [《守望先锋》架构设计和网络同步](https://www.lfzxb.top/ow-gdc-gameplay-architecture-and-netcode/)(非帧同步)
- [ACT类游戏 帧同步及预表现技术分享](http://awucn.cn/?p=597)
- [了解格斗游戏网络](http://mauve.mizuumi.net/2012/07/05/understanding-fighting-game-networking/)
- [帧同步联机战斗（预测，快照，回滚）](https://blog.csdn.net/a673544319/article/details/81697643)
- [Lockstep Engine](https://github.com/JiepengTan/LockstepEngine)（帧同步引擎）
- [Lockstep Tutorial](https://github.com/JiepengTan/Lockstep-Tutorial)（帧同步引擎教学）

- [FixedPoint-Sharp](https://github.com/RomanZhu/FixedPoint-Sharp)（C#定点数，2D数学库）
- [Deterministic2DPhysics](https://github.com/iaincarsberg/Deterministic2DPhysics)（确定性2D物理，Svelto.ECS相关）
- [Svelto.Demo.FixedMath](https://github.com/sebas77/Svelto.MiniExamples/tree/26ffc36a041eca7ba5d6d2824b7fe09947023c0e/Example4-NET-SDL/FixedMath/FixedMaths)（Svelto Demo中的确定性2D数学库）
- [bepuphysics2](https://github.com/bepu/bepuphysics2)（非确定性3D物理）

### 注意点
- float导致不同设备运行结果不一致。
  
- 多线程，不能保证逻辑执行顺序，可能导致运行结果不一致问题。逻辑代码尽量不使用多线程。
  
- 三种逻辑运行方式
    
    - 单机， 输入 -> 逻辑更新 -> 表现
    - 状态同步，输入 -> 发送给服务器 -> 同步结果给客户端 -> 客户端插值
    - 帧同步，输入 -> 发送给服务器 -> （同时进行客户端预测）-> 服务器按帧收集所有客户端输入 -> 分发给客户端 -> 客户端进行模拟 
    - 预测 回滚

### 帧同步相关：

- 客户端基于预测、回滚方式实现帧同步，客户端收到的服务器的帧一定是结果帧，若没收到当前帧数据，则沿用上一帧，或者忽略。
- 预测 指的是本地对其他客户端当前帧操作进行预测，等待服务器同步其他客户端操作回来时，再对此帧操作进行验证，若不同则进行回滚操作（本地玩家不需要预测回滚）。
- 当服务器收到全部客户端的当前帧输入时，不需要等待帧的时间到达，直接转发消息给客户端。