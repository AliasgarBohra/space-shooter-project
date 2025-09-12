mergeInto(LibraryManager.library, {
  PostMatchMessageJS: function (strPtr) {
    var json = UTF8ToString(strPtr);
    try {
      var payload = JSON.parse(json);
      if (window && window.parent) {
        window.parent.postMessage(payload, "https://aliasgarbohra.github.io");
      }
    } catch (e) {
      if (window && window.parent) {
        window.parent.postMessage(
          { type: 'match_abort', payload: { message: 'Invalid JSON', error: e.toString() } },
          "*"
        );
      }
    }
  },

  SetBrowserUrl: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    if (window && window.history && window.history.pushState) {
      window.history.pushState({}, '', url);
    }
  }
});
