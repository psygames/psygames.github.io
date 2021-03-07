## 文章

- [Lockstep Engine](https://github.com/JiepengTan/LockstepEngine)（帧同步引擎）
- [Lockstep Tutorial](https://github.com/JiepengTan/Lockstep-Tutorial)（帧同步引擎教学）

- [FixedPoint-Sharp](https://github.com/RomanZhu/FixedPoint-Sharp)（C#定点数）
- [Deterministic2DPhysics](https://github.com/iaincarsberg/Deterministic2DPhysics)（确定性2D物理，Svelto.ECS相关）
- [Svelto.Demo.FixedMath](https://github.com/sebas77/Svelto.MiniExamples/tree/26ffc36a041eca7ba5d6d2824b7fe09947023c0e/Example4-NET-SDL/FixedMath/FixedMaths)（Svelto Demo中的确定性2D物理）
- [bepuphysics2](https://github.com/bepu/bepuphysics2)（确定性3D物理）
- [ACT类游戏 帧同步及预表现技术分享](http://awucn.cn/?p=597)

### 注意点
- float导致不同设备运行结果不一致。
  
- 多线程，不能保证逻辑执行顺序，可能导致运行结果不一致问题。逻辑代码尽量不使用多线程。
  
- 三种逻辑运行方式
    
    - 单机， 输入 -> 逻辑更新 -> 表现
    - 状态同步，输入 -> 发送给服务器 -> 同步结果给客户端 -> 客户端插值
    - 帧同步，输入 -> 发送给服务器 -> （同时进行客户端预测）-> 服务器按帧收集所有客户端输入 -> 分发给客户端 -> 客户端进行模拟 
    - 预测 回滚

### 帧同步相关：

- 客户端基于预测、回滚方式实现帧同步，客户端收到的服务器的帧一定是结果帧，若没收到当前帧数据，则进行预测。

- 当服务器收到全部客户端的当前帧输入时，不需要等待帧的时间到达，直接转发消息给客户端。这里可以设置一个接受客户端输入缓冲帧数，超出缓冲帧，某客户端消息并未到达，则服务器进行预测，并且作为最终结果，同步给其他客户端。