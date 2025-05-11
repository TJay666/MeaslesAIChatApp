# 台灣疾管署文件麻疹 AI 問答助理 (Taiwan CDC Document AI Chat Assistant)

這是基於 .NET 開發的 AI 聊天應用程式，旨在展示如何利用 **檢索增強生成 (Retrieval-Augmented Generation, RAG)** 技術，讓大型語言模型 (LLM) 能夠根據自訂的專業文件資料（初期以台灣疾管署的麻疹防治工作手冊及相關指引 PDF 為例）來回答問題。

本專案為使用者（特別是公共衛生及流行病學領域的工作人員）提供一個快速查詢官方文件內容的輔助工具，例如查詢特定傳染病的潛伏期、病例定義、防疫措施等。

> [!NOTE]
> 此專案最初基於 ".NET AI Chat Web App Template Preview 2" 範本開發，目前仍處於持續開發與優化階段。如果您有任何回饋，歡迎提出！

## 專案特色

* **RAG 技術核心：** 整合文件擷取、文本切割、向量化、向量儲存、語意搜尋及 LLM 生成，提供基於特定文件的問答能力。
* **在地化知識庫：** 使用台灣疾管署官方 PDF 文件作為知識來源。
* **Blazor Server UI：** 提供互動式的 Web 聊天介面。
* **.NET 全端開發：** 使用 C# 語言進行前端與後端開發。
* **雲端部署：** 設計為可部署至 Microsoft Azure App Service。
* **安全組態管理：** 強調機密資料（如 API Token）的安全管理。

## 設定 AI 模型供應商

本專案透過 `Services/GitHubModelService.cs`（可依需求更名或替換）中的 `HttpClient` 直接呼叫大型語言模型的 API 端點。您需要設定以下組態才能讓應用程式正常運作：

1.  **AI 模型 API 端點 (API Endpoint)：**
    * 這是您選擇的 AI 模型服務提供的 API 網址。
    * 在 `appsettings.json` 中設定或透過環境變數設定。
        * 鍵名 (Key): `GitHubModels:ApiEndpoint`
        * 預設值 (若未設定): `https://models.github.ai/inference` (此為範例，請替換成您實際使用的端點)

2.  **AI 模型名稱 (Model Name)：**
    * 您希望使用的具體模型識別碼。
    * 在 `appsettings.json` 中設定或透過環境變數設定。
        * 鍵名 (Key): `GitHubModels:ModelName`
        * 預設值 (若未設定): `openai/o3` (此為範例，請替換成您實際使用的模型)

3.  **API Token/Key：**
    * 存取 AI 模型 API 所需的驗證權杖或金鑰。**此為敏感資訊，切勿直接寫入 `appsettings.json` 或 commit 到 Git。**

    * **本地開發 (Local Development)：**
        建議使用 .NET User Secrets 來設定。在您的專案根目錄 (`MeaslesAIChatApp`) 下開啟終端機，執行以下指令 (將 `YOUR_ACTUAL_API_TOKEN` 替換成您真實的 Token)：
        ```sh
        cd MeaslesAIChatApp
        dotnet user-secrets set GitHubModels:Token YOUR_ACTUAL_API_TOKEN
        ```
        若要設定其他組態 (非機密，或本地開發用)，也可以用類似方式：
        ```sh
        dotnet user-secrets set GitHubModels:ApiEndpoint YOUR_LOCAL_DEV_ENDPOINT
        dotnet user-secrets set GitHubModels:ModelName YOUR_LOCAL_DEV_MODEL
        ```

    * **部署至 Azure App Service：**
        請在 Azure App Service 的 "Configuration" -> "Application settings" 中新增以下環境變數：
        * `GitHubModels__Token` (值為您的 API Token)
        * `GitHubModels__ApiEndpoint` (值為您的 API 端點 URL)
        * `GitHubModels__ModelName` (值為您使用的模型名稱)
        * `ASPNETCORE_ENVIRONMENT` (值設為 `Production`)

## 資料來源

* 本專案預期從 `wwwroot/Data/` 資料夾中讀取 PDF 文件作為知識庫來源。
* 目前的資料擷取服務 (`PDFDirectorySource.cs`) 會處理此資料夾下的所有 `.pdf` 檔案。
* 執行資料擷取後，處理過的文本區塊及其向量會儲存在 `vector-store/data-measlesaichatapp-ingested.json` 檔案中。
    > [!IMPORTANT]
    > 當您新增、修改或刪除 `wwwroot/Data/` 中的 PDF 檔案後，需要**重新執行資料擷取流程**以更新 `data-measlesaichatapp-ingested.json`，AI 助理才能使用最新的文件內容。預設模板可能需要在本地執行此擷取步驟，然後將更新後的 JSON 檔案部署。

## 如何執行 (本地開發)

1.  **設定組態：** 確保已依照上述「設定 AI 模型供應商」的說明，使用 .NET User Secrets 設定了 `GitHubModels:Token` (以及視需要的 `ApiEndpoint` 和 `ModelName`)。
2.  **還原 NuGet 套件：**
    ```sh
    dotnet restore
    ```
3.  **執行資料擷取 (首次執行或文件更新後)：**
    * 目前專案的資料擷取流程 (`DataIngestor.cs`) 可能需要在應用程式啟動時被呼叫，或您需要設計一個手動觸發的方式。請檢查 `Program.cs` 或相關服務的初始化邏輯，確保擷取流程能被執行以產生 `data-measlesaichatapp-ingested.json`。
4.  **執行應用程式：**
    ```sh
    dotnet run
    ```
    或透過 VS Code 啟動偵錯。
5.  開啟瀏覽器並前往應用程式顯示的 URL (通常是 `https://localhost:XXXX` 或 `http://localhost:YYYY`)。

## 主要技術棧

* **.NET 9** (或您專案設定的 .NET 版本)
* **ASP.NET Core**
* **Blazor Server**
* **C#**
* **Azure App Service** (部署平台)
* 用於 RAG 的相關概念 (文字切割、向量嵌入、語意搜尋)
* `System.Net.Http.HttpClient` (用於呼叫外部 API)
* `System.Text.Json` (用於 JSON 處理)

---
