# DescFix

SPT 4.1.x 服务端模组：补全游戏中文语言包里过度简略的物品简介。

## 为什么需要

EFT 的俄语文本由开发商自己维护，其他语言多为外包翻译。对照俄语后发现，中文包里有一批物品简介只剩一个名词，甚至保留了开发期占位符：

- 弹药：`.45 ACP Hydra-Shok` 只剩“.45 ACP 中柱空尖弹。”，弹头重量、材质、用途全部丢失；12.7x108 B-32 只有一句“12.7x108毫米弹。”。
- 军帽：五种迷彩的帽子简介全都是“军帽”。
- 配件：TOZ-106 基座漏了“需先装燕尾槽基座”这条使用前提；多个枪托、护木、瞄具丢了厂商和用途。
- 占位符：AGS-30 弹箱、PAG-17 瞄准镜、VOG-30 的简介是字面上的“EDIT”；两个 FN40GL 枪托整段保留英文。

## 做了什么

只做一件事：服务端启动时，用 `db/ch.json` 里的 193 条文本覆盖中文语言包里对应的 `<物品ID> Description`。

- 译文以俄语为准、英文为辅，保持原有的物品名称不变。
- 只改简介文本，不读取、不修改任何物品、任务、商人、藏身处数据，不改游戏逻辑。
- 只覆盖 `ch` 语言；其他语言不受影响。

本轮新增 89 条，物品 ID、游戏内名称与遗漏信息见[复核清单](AUDIT.md)。

## 安装

把构建输出放到服务端模组目录：

```
SPT_Runtime/user/mods/DescFix/
├── DescFix.Server.dll
└── db/ch.json
```

重启服务端后，日志应出现 `DescFix: 193 Chinese descriptions applied`，表示已注册 193 条中文简介覆盖。

## 构建

```bash
dotnet build -c Release DescFix.Server/DescFix.Server.csproj
```

输出在 `DescFix.Server/bin/Release/DescFix.Server/`，整个文件夹复制过去即可。

## 添加或修改译文

编辑 `DescFix.Server/db/ch.json`，格式是平铺的 `"<物品ID> Description": "译文"`。物品 ID 可在 `SPT_Runtime/SPT_Data/database/locales/global/ch.json` 里查到。把更新后的 `db/ch.json` 同步到服务端模组目录后重启服务端，无需重新编译。

## 兼容性

- 注册时机较晚（`OnLoadOrder.PostLoad`），其他模组新增的语言条目不会被覆盖，也不会覆盖它们，除非 key 相同。
- 如果某个 key 在你的语言包里不存在，会被直接写入，不会报错。
- 不影响存档。卸载只需删除模组文件夹。

## 许可证

本项目以 **CC0 1.0 Universal** 协议放弃版权并奉献至公共领域，详见 [LICENSE](LICENSE)。
