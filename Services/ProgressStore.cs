using System.Text.Json;
using Microsoft.JSInterop;

namespace WhatTheCode.Services
{
    /// <summary>
    /// Saves best stars per level in the browser's localStorage.
    /// Stand-in until real accounts land; swap the storage calls for an API later.
    /// </summary>
    public class ProgressStore
    {
        private const string StorageKey = "whatthecode.progress.v1";
        private readonly IJSRuntime _js;
        private Dictionary<int, int> _stars = new();

        public ProgressStore(IJSRuntime js) => _js = js;

        public bool IsLoaded { get; private set; }

        public int StarsFor(int level) => _stars.GetValueOrDefault(level);

        public bool IsSolved(int level) => _stars.ContainsKey(level);

        public bool IsUnlocked(int level) => level == 1 || IsSolved(level - 1);

        public async Task LoadAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                    _stars = JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new();
            }
            catch (Exception ex) when (ex is JSException or JsonException)
            {
                _stars = new(); // corrupt or blocked storage: start fresh rather than crash
            }
            IsLoaded = true;
        }

        /// <summary>Keeps the best result. Returns true if this beat the previous best.</summary>
        public async Task<bool> RecordAsync(int level, int stars)
        {
            if (stars <= StarsFor(level)) return false;

            _stars[level] = stars;
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(_stars));
            }
            catch (JSException)
            {
                // Storage unavailable (private browsing etc.). Progress still counts for this visit.
            }
            return true;
        }
    }
}
