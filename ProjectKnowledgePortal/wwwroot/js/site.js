(() => {
  const themeStorageKey = 'pkp-theme';

  const applyTheme = (theme) => {
    const normalized = theme === 'dark' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', normalized);
    try {
      localStorage.setItem(themeStorageKey, normalized);
    } catch {
    }
  };

  document.addEventListener('click', (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) {
      return;
    }

    const trigger = target.closest('.theme-toggle-btn');
    if (!trigger) {
      return;
    }

    const selectedTheme = trigger.getAttribute('data-theme-value');
    if (selectedTheme === 'light' || selectedTheme === 'dark') {
      applyTheme(selectedTheme);
    }
  });
})();
