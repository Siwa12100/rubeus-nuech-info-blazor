// Snake Game Keyboard Handler
window.SnakeGame = window.SnakeGame || {};

SnakeGame.InitializeKeyboardListener = function(dotnetHelper) {
    console.log("Snake Game Keyboard Listener Initialized");

    document.addEventListener('keydown', function(e) {
        // Empêcher le défilement par défaut
        switch(e.key.toLowerCase()) {
            case 'arrowup':
            case 'arrowdown':
            case 'arrowleft':
            case 'arrowright':
            case ' ':
            case 'enter':
            case 'w':
            case 'a':
            case 's':
            case 'd':
                e.preventDefault();
                break;
        }

        // Appeler les méthodes Blazor
        switch(e.key.toLowerCase()) {
            case 'arrowup':
                dotnetHelper.invokeMethodAsync('HandleArrowUp');
                break;
            case 'arrowdown':
                dotnetHelper.invokeMethodAsync('HandleArrowDown');
                break;
            case 'arrowleft':
                dotnetHelper.invokeMethodAsync('HandleArrowLeft');
                break;
            case 'arrowright':
                dotnetHelper.invokeMethodAsync('HandleArrowRight');
                break;
            case 'w':
                dotnetHelper.invokeMethodAsync('HandleW');
                break;
            case 's':
                dotnetHelper.invokeMethodAsync('HandleS');
                break;
            case 'a':
                dotnetHelper.invokeMethodAsync('HandleA');
                break;
            case 'd':
                dotnetHelper.invokeMethodAsync('HandleD');
                break;
            case ' ':
                dotnetHelper.invokeMethodAsync('HandleSpace');
                break;
            case 'enter':
                dotnetHelper.invokeMethodAsync('HandleEnter');
                break;
        }
    }, true); // Utiliser la phase de capture pour intercepter les événements plus tôt
};

// Easter Egg: Konami-like code to open Snake Game Modal
// Sequence: S-N-A-K-E to activate
window.snakeEasterEggSequence = [];
window.snakeEasterEggCode = ['s', 'n', 'a', 'k', 'e'];
window.snakeEasterEggTimeout;

window.initializeSnakeEasterEgg = function(dotnetHelper) {
    console.log("Snake Easter Egg Initialized - Press S-N-A-K-E to activate");
    
    document.addEventListener('keydown', function(e) {
        const key = e.key.toLowerCase();
        
        // Reset sequence after 3 seconds of inactivity
        clearTimeout(window.snakeEasterEggTimeout);
        window.snakeEasterEggTimeout = setTimeout(() => {
            window.snakeEasterEggSequence = [];
        }, 3000);
        
        // Check if this key is expected
        if (key === window.snakeEasterEggCode[window.snakeEasterEggSequence.length]) {
            window.snakeEasterEggSequence.push(key);
            console.log(`Easter Egg Progress: ${window.snakeEasterEggSequence.join('-')}`);
            
            // Check if complete sequence is matched
            if (window.snakeEasterEggSequence.length === window.snakeEasterEggCode.length) {
                console.log("🐍 SNAKE EASTER EGG ACTIVATED! 🐍");
                window.snakeEasterEggSequence = [];
                dotnetHelper.invokeMethodAsync('OpenSnakeEasterEgg');
            }
        } else {
            // Reset if wrong key
            window.snakeEasterEggSequence = [];
            if (key === window.snakeEasterEggCode[0]) {
                window.snakeEasterEggSequence.push(key);
            }
        }
    });
};

